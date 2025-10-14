using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;

namespace TaskTamer_Application.Service
{
    public class EmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IPositionRepository _positionRepository;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public EmployeeService(IEmployeeRepository employeeRepository, IDepartmentRepository departmentRepository, IPositionRepository positionRepository)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _positionRepository = positionRepository;
        }

        public async Task<OperationResult<int>> CreateEmployeeAsync(EmployeeDTO employeeDto)
        {
            try
            {
                _logger.Info($"Создание сотрудника: {employeeDto.FullName}");
                if (employeeDto == null)
                {
                    _logger.Warn("Попытка создания пустого сотрудника");
                    return OperationResult<int>.Failure("Данные сотрудника не предоставлены");
                }
                // Валидация данных
                var validationResult = ValidateEmployeeDto(employeeDto);
                if (!validationResult.IsSuccess)
                {
                    _logger.Warn($"Ошибка валидации: {validationResult.Message}");
                    return OperationResult<int>.Failure(validationResult.Message);
                }
                // Проверка существования должности и отдела
                var position = await _positionRepository.GetByIdAsync(employeeDto.positionDTO.PositionID);
                if (position == null)
                {
                    _logger.Warn($"Должность с ID {employeeDto.positionDTO.PositionID} не найдена");
                    return OperationResult<int>.Failure("Указанная должность не найдена");
                }

                var department = await _departmentRepository.GetByIdAsync(employeeDto.departmentDTO.DepartmentID);
                if (department == null)
                {
                    _logger.Warn($"Отдел с ID {employeeDto.departmentDTO.DepartmentID} не найден");
                    return OperationResult<int>.Failure("Указанный отдел не найден");
                }

                // Преобразование DTO в entity
                var employee = new Employee
                {
                    FullName = employeeDto.FullName.Trim(),
                    PositionID = employeeDto.positionDTO.PositionID,
                    DepartmentID = employeeDto.departmentDTO.DepartmentID,
                    Phone = employeeDto.Phone?.Trim(),
                    Email = employeeDto.Email?.Trim(),
                    UserType = employeeDto.UserType,
                    RegistrationDate = DateTime.UtcNow,
                    TerminationDate = null,
                    IsActive = true
                };

                var employeeId = await _employeeRepository.AddAsync(employee);

                _logger.Info($"Сотрудник {employeeDto.FullName} создан с ID: {employeeId}");
                return OperationResult<int>.Success(employeeId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при создании сотрудника {employeeDto?.FullName}");
                return OperationResult<int>.Failure($"Ошибка при создании сотрудника {ex.Message}");
            }
        }

        public async Task<OperationResult> UpdateEmployeeAsync(EmployeeDTO employeeDto)
        {
            try
            {
                _logger.Info($"Обновление сотрудника с ID: {employeeDto.EmployeeID}");

                if (employeeDto == null)
                {
                    _logger.Warn("Попытка обновления пустого сотрудника");
                    return OperationResult.Failure("Данные сотрудника не предоставлены");
                }

                // Валидация данных
                var validationResult = ValidateEmployeeDto(employeeDto);
                if (!validationResult.IsSuccess)
                {
                    _logger.Warn($"Ошибка валидации: {validationResult.Message}");
                    return OperationResult.Failure(validationResult.Message);
                }

                // Проверка существования сотрудника
                var existingEmployee = await _employeeRepository.GetByIdAsync(employeeDto.EmployeeID);
                if (existingEmployee == null)
                {
                    _logger.Warn($"Сотрудник с ID {employeeDto.EmployeeID} не найден");
                    return OperationResult.Failure("Сотрудник не найден");
                }

                // Проверка существования должности и отдела
                var position = await _positionRepository.GetByIdAsync(employeeDto.positionDTO.PositionID);
                if (position == null)
                {
                    _logger.Warn($"Должность с ID {employeeDto.positionDTO.PositionID} не найдена");
                    return OperationResult.Failure("Указанная должность не найдена");
                }

                var department = await _departmentRepository.GetByIdAsync(employeeDto.departmentDTO.DepartmentID);
                if (department == null)
                {
                    _logger.Warn($"Отдел с ID {employeeDto.departmentDTO.DepartmentID} не найден");
                    return OperationResult.Failure("Указанный отдел не найден");
                }

                // Обновление данных
                existingEmployee.FullName = employeeDto.FullName.Trim();
                existingEmployee.PositionID = employeeDto.positionDTO.PositionID;
                existingEmployee.DepartmentID = employeeDto.departmentDTO.DepartmentID;
                existingEmployee.Phone = employeeDto.Phone?.Trim();
                existingEmployee.Email = employeeDto.Email?.Trim();
                existingEmployee.UserType = employeeDto.UserType;
                existingEmployee.IsActive = employeeDto.IsActive;
                existingEmployee.TerminationDate = employeeDto.TerminationDate;

                var result = await _employeeRepository.UpdateAsync(existingEmployee);

                if (result > 0)
                {
                    _logger.Info($"Сотрудник с ID {employeeDto.EmployeeID} успешно обновлен");
                    return OperationResult.Success($"Сотрудник с ID {employeeDto.EmployeeID} успешно обновлен");
                }
                else
                {
                    _logger.Warn($"Не удалось обновить сотрудника с ID {employeeDto.EmployeeID}");
                    return OperationResult.Failure("Не удалось обновить сотрудника");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при обновлении сотрудника с ID {employeeDto?.EmployeeID}");
                return OperationResult.Failure("Ошибка при обновлении сотрудника");
            }
        }

        public async Task<OperationResult> DeleteEmployeeAsync(int id)
        {
            try
            {
                _logger.Info($"Удаление сотрудника с ID: {id}");

                if (id <= 0)
                {
                    _logger.Warn("Неверный ID сотрудника для удаления");
                    return OperationResult.Failure("Неверный идентификатор сотрудника");
                }

                var existingEmployee = await _employeeRepository.GetByIdAsync(id);
                if (existingEmployee == null)
                {
                    _logger.Warn($"Сотрудник с ID {id} не найден для удаления");
                    return OperationResult.Failure("Сотрудник не найден");
                }

                var result = await _employeeRepository.DeleteAsync(id);

                if (result > 0)
                {
                    _logger.Info($"Сотрудник с ID {id} успешно удален");
                    return OperationResult.Success($"Сотрудник с ID {id} успешно удален");
                }
                else
                {
                    _logger.Warn($"Не удалось удалить сотрудника с ID {id}");
                    return OperationResult.Failure("Не удалось удалить сотрудника");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при удалении сотрудника с ID {id}");
                return OperationResult.Failure("Ошибка при удалении сотрудника");
            }
        }

        public async Task<OperationResult<EmployeeDTO>> GetEmployeeByIdAsync(int id)
        {
            try
            {
                _logger.Debug($"Запрос сотрудника с ID: {id}");

                if (id <= 0)
                {
                    _logger.Warn("Неверный ID сотрудника для запроса");
                    return OperationResult<EmployeeDTO>.Failure("Неверный идентификатор сотрудника");
                }

                var employee = await _employeeRepository.GetByIdAsync(id);

                if (employee == null)
                {
                    _logger.Warn($"Сотрудник с ID {id} не найден");
                    return OperationResult<EmployeeDTO>.Failure("Сотрудник не найден");
                }

                var employeeDto = new EmployeeDTO(employee);
                _logger.Debug($"Сотрудник с ID {id} успешно получен");
                return OperationResult<EmployeeDTO>.Success(employeeDto);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при получении сотрудника с ID {id}");
                return OperationResult<EmployeeDTO>.Failure("Ошибка при получении сотрудника");
            }
        }

        public async Task<OperationResult<IEnumerable<EmployeeDTO>>> GetAllEmployeesAsync()
        {
            try
            {
                _logger.Debug("Запрос всех сотрудников");

                var employees = await _employeeRepository.GetAllAsync();

                if (employees == null || !employees.Any())
                {
                    _logger.Info("Сотрудники не найдены");
                    return OperationResult<IEnumerable<EmployeeDTO>>.Success(Enumerable.Empty<EmployeeDTO>());
                }

                var employeeDtos = employees.Select(e => new EmployeeDTO(e)).ToList();
                _logger.Debug($"Получено {employeeDtos.Count} сотрудников");
                return OperationResult<IEnumerable<EmployeeDTO>>.Success(employeeDtos);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при получении списка сотрудников");
                return OperationResult<IEnumerable<EmployeeDTO>>.Failure("Ошибка при получении списка сотрудников");
            }
        }

        private OperationResult ValidateEmployeeDto(EmployeeDTO employeeDto)
        {
            if (string.IsNullOrWhiteSpace(employeeDto.FullName))
                return OperationResult.Failure("ФИО сотрудника обязательно");

            if (employeeDto.FullName.Length > 100)
                return OperationResult.Failure("ФИО сотрудника слишком длинное");

            if (employeeDto.positionDTO == null || employeeDto.positionDTO.PositionID <= 0)
                return OperationResult.Failure("Должность обязательна");

            if (employeeDto.departmentDTO == null || employeeDto.departmentDTO.DepartmentID <= 0)
                return OperationResult.Failure("Отдел обязателен");

            if (!string.IsNullOrWhiteSpace(employeeDto.Email) && !IsValidEmail(employeeDto.Email))
                return OperationResult.Failure("Неверный формат email");

            if (!string.IsNullOrWhiteSpace(employeeDto.Phone) && employeeDto.Phone.Length > 20)
                return OperationResult.Failure("Телефон слишком длинный");

            return OperationResult.Success("Данные корректны");
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}
