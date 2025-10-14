using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;

namespace TaskTamer_Application.Service;

public class DepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public DepartmentService(IDepartmentRepository departmentRepository, IEmployeeRepository employeeRepository)
    {
        _departmentRepository = departmentRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<OperationResult<int>> CreateDepartmentAsync(DepartmentDTO depDto)
    {
        try
        {
            _logger.Info($"Создание департамента: {depDto.Name}");
            if (depDto == null)
            {
                _logger.Warn("Попытка создания пустого департамента");
                return OperationResult<int>.Failure("Данные департамент не предоставлены");
            }

            var validationResult = ValidateDepartmentDto(depDto);
            if (!validationResult.IsSuccess)
            {
                _logger.Warn($"Ошибка валидации: {validationResult.Message}");
                return OperationResult<int>.Failure(validationResult.Message);
            }

            var existingDep = await _departmentRepository.GetByNameAsync(depDto.Name);
            if (existingDep != null)
            {
                _logger.Warn($"Департамент с именем '{depDto.Name}' уже существует");
                return OperationResult<int>.Failure("Департамент с таким именем уже существует");
            }
            var department = new Department
            {
                Name = depDto.Name.Trim(),
                Description = depDto.Description?.Trim(),
                IsActive = depDto.IsActive,
                CreationDate = depDto.CreationDate,
                DepartmentType = depDto.DepartmentType
            };

            var departmentId = await _departmentRepository.AddAsync(department);

            _logger.Info($"Департамент '{depDto.Name}' создана с ID: {departmentId}");
            return OperationResult<int>.Success(departmentId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при создании департамента '{depDto?.Name}'");
            return OperationResult<int>.Failure("Ошибка при создании департамента");
        }
    }

    public async Task<OperationResult<DepartmentDTO>> GetDepartmentByNameAsync(string name)
    {
        try
        {
            _logger.Debug($"Запрос департамента по имени: {name}");
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.Warn("Неверное имя департамента для запроса");
                return OperationResult<DepartmentDTO>.Failure("Имя департамента не указано");
            }
            var pos = await _departmentRepository.GetByNameAsync(name.Trim());
            if (pos == null)
            {
                _logger.Warn($"Департамент с именем '{name}' не найдена");
                return OperationResult<DepartmentDTO>.Failure("Департамент не найдена");
            }
            var posDto = new DepartmentDTO(pos);
            _logger.Debug($"Департамент с именем '{name}' успешно получена");
            return OperationResult<DepartmentDTO>.Success(posDto);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при получении департамента с именем '{name}'");
            return OperationResult<DepartmentDTO>.Failure("Ошибка при получении департамента");
        }
    }

    public async Task<OperationResult<IEnumerable<DepartmentDTO>>> GetAllDepartmentsAsync()
    {
        try
        {
            _logger.Debug("Запрос всех департаментов");

            var positions = await _departmentRepository.GetAllAsync();

            if (positions == null || !positions.Any())
            {
                _logger.Info("Департаменты не найдены");
                return OperationResult<IEnumerable<DepartmentDTO>>.Success(Enumerable.Empty<DepartmentDTO>());
            }

            var posDtos = positions.Select(p => new DepartmentDTO(p)).ToList();
            _logger.Debug($"Получено {posDtos.Count} департаментов");
            return OperationResult<IEnumerable<DepartmentDTO>>.Success(posDtos);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при получении списка департаментов");
            return OperationResult<IEnumerable<DepartmentDTO>>.Failure("Ошибка при получении списка департаментов");
        }
    }

    public async Task<OperationResult> UpdateDepartmentAsync(DepartmentDTO depDto)
    {
        try
        {
            _logger.Info($"Обновление департамента с ID: {depDto.DepartmentID}");
            if (depDto == null)
            {
                _logger.Warn("Попытка обновления пустого департамента");
                return OperationResult.Failure("Данные департамента не предоставлены");
            }

            var validationResult = ValidateDepartmentDto(depDto);
            if (!validationResult.IsSuccess)
            {
                _logger.Warn($"Ошибка валидации: {validationResult.Message}");
                return OperationResult.Failure(validationResult.Message);
            }

            var existingDep = await _departmentRepository.GetByIdAsync(depDto.DepartmentID);
            if (existingDep == null)
            {
                _logger.Warn($"Департамент с ID {depDto.DepartmentID} не найдена");
                return OperationResult.Failure("Департамент не найдена");
            }

            if (!string.Equals(existingDep.Name, depDto.Name, StringComparison.OrdinalIgnoreCase))
            {
                var depWithSameName = await _departmentRepository.GetByNameAsync(depDto.Name);
                if (depWithSameName != null)
                {
                    _logger.Warn($"Департамент с именем '{depDto.Name}' уже существует");
                    return OperationResult.Failure("Департамент с таким именем уже существует");
                }
            }

            existingDep.Name = depDto.Name.Trim();
            existingDep.Description = depDto.Description?.Trim();
            existingDep.IsActive = depDto.IsActive;
            existingDep.CreationDate = depDto.CreationDate;

            var result = await _departmentRepository.UpdateAsync(existingDep);
            if (result > 0)
            {
                _logger.Info($"Департамент с ID {depDto.DepartmentID} успешно обновлена");
                return OperationResult.Success();
            }
            else
            {
                _logger.Warn($"Не удалось обновить департамент с ID {depDto.DepartmentID}");
                return OperationResult.Failure("Не удалось обновить департамент");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при обновлении департамента с ID {depDto.DepartmentID}");
            return OperationResult.Failure("Ошибка при обновлении департамента");
        }
    }
    public async Task<OperationResult> DeleteDepartmentAsync(int id)
    {
        try
        {
            _logger.Info($"Удаление департамента с ID: {id}");
            if (id <= 0)
            {
                _logger.Warn("Неверный ID департамента для удаления");
                return OperationResult.Failure("Неверный идентификатор департамента");
            }

            var existingDep = await _departmentRepository.GetByIdAsync(id);
            if (existingDep == null)
            {
                _logger.Warn($"Департамент с ID {id} не найдена для удаления");
                return OperationResult.Failure("Департамент не найдена");
            }
            var empWithDep = await _employeeRepository.GetEmployeeWithPositionAsync(id);
            if (empWithDep.Any())
            {
                _logger.Warn($"Невозможно удалить департамент с ID {id} - она используется пользователями");
                return OperationResult.Failure("Невозможно удалить департамент - она используется пользователями");
            }

            var result = await _departmentRepository.DeleteAsync(id);
            if (result > 0)
            {
                _logger.Info($"Департамент с ID {id} успешно удалена");
                return OperationResult.Success();
            }
            else
            {
                _logger.Warn($"Не удалось удалить департамент с ID {id}");
                return OperationResult.Failure("Не удалось удалить департамент");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при удалении департамента с ID {id}");
            return OperationResult.Failure("Ошибка при удалении департамента");
        }
    }
    public async Task<OperationResult<DepartmentDTO>> GetDepartmentByIdAsync(int id)
    {
        try
        {
            _logger.Debug($"Запрос департамента с ID: {id}");
            if (id <= 0)
            {
                _logger.Warn("Неверный ID департамента для запроса");
                return OperationResult<DepartmentDTO>.Failure("Неверный идентификатор департамента");
            }
            var dep = await _departmentRepository.GetByIdAsync(id);
            if (dep == null)
            {
                _logger.Warn($"Департамент с ID {id} не найдена");
                return OperationResult<DepartmentDTO>.Failure("Департамент не найдена");
            }
            var depDto = new DepartmentDTO(dep);
            _logger.Debug($"Департамент с ID {id} успешно получена");
            return OperationResult<DepartmentDTO>.Success(depDto);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при получении департамента с ID {id}");
            return OperationResult<DepartmentDTO>.Failure("Ошибка при получении департамента");
        }
    }
    private OperationResult ValidateDepartmentDto(DepartmentDTO depDto)
    {
        if (string.IsNullOrWhiteSpace(depDto.Name))
            return OperationResult.Failure("Название департамента обязательно");

        if (depDto.Name.Length > 50)
            return OperationResult.Failure("Название департамента слишком длинное");

        if (!string.IsNullOrWhiteSpace(depDto.Description) && depDto.Description.Length > 200)
            return OperationResult.Failure("Описание департамента слишком длинное");

        return OperationResult.Success();
    }
}
