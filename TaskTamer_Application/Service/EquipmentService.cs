using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;

namespace TaskTamer_Application.Service;

public class EquipmentService
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IRequestRepository _requestRepository;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public EquipmentService(
        IEquipmentRepository equipmentRepository,
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        IRequestRepository requestRepository)
    {
        _equipmentRepository = equipmentRepository;
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _requestRepository = requestRepository;
    }

    public async Task<OperationResult<EquipmentDTO>> GetEquipmentByIdAsync(int id)
    {
        try
        {
            _logger.Debug($"Запрос оборудования с ID: {id}");

            if (id <= 0)
            {
                _logger.Warn("Неверный ID оборудования для запроса");
                return OperationResult<EquipmentDTO>.Failure("Неверный идентификатор оборудования");
            }

            var equipment = await _equipmentRepository.GetByIdAsync(id);

            if (equipment == null)
            {
                _logger.Warn($"Оборудование с ID {id} не найдено");
                return OperationResult<EquipmentDTO>.Failure("Оборудование не найдено");
            }

            var equipmentDto = new EquipmentDTO(equipment);
            _logger.Debug($"Оборудование с ID {id} успешно получено");
            return OperationResult<EquipmentDTO>.Success(equipmentDto);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при получении оборудования с ID {id}");
            return OperationResult<EquipmentDTO>.Failure("Ошибка при получении оборудования");
        }
    }


    public async Task<OperationResult<IEnumerable<EquipmentDTO>>> GetAllEquipmentAsync()
    {
        try
        {
            _logger.Debug("Запрос всего оборудования");

            var equipment = await _equipmentRepository.GetAllAsync();

            if (equipment == null || !equipment.Any())
            {
                _logger.Info("Оборудование не найдено");
                return OperationResult<IEnumerable<EquipmentDTO>>.Success(Enumerable.Empty<EquipmentDTO>());
            }

            var equipmentDtos = equipment.Select(e => new EquipmentDTO(e)).ToList();
            _logger.Debug($"Получено {equipmentDtos.Count} единиц оборудования");
            return OperationResult<IEnumerable<EquipmentDTO>>.Success(equipmentDtos);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при получении списка оборудования");
            return OperationResult<IEnumerable<EquipmentDTO>>.Failure("Ошибка при получении списка оборудования");
        }
    }


    public async Task<OperationResult<int>> CreateEquipmentAsync(EquipmentDTO equipmentDto)
    {
        try
        {
            _logger.Info($"Создание оборудования: {equipmentDto.Name}");

            if (equipmentDto == null)
            {
                _logger.Warn("Попытка создания пустого оборудования");
                return OperationResult<int>.Failure("Данные оборудования не предоставлены");
            }

            // Валидация данных
            var validationResult = ValidateEquipmentDto(equipmentDto);
            if (!validationResult.IsSuccess)
            {
                _logger.Warn($"Ошибка валидации оборудования: {validationResult.Message}");
                return OperationResult<int>.Failure(validationResult.Message);
            }

            // Проверка уникальности серийного номера
            if (!string.IsNullOrWhiteSpace(equipmentDto.SerialNumber))
            {
                var existingWithSerial = (await _equipmentRepository.GetAllAsync())
                    .FirstOrDefault(e => e.SerialNumber == equipmentDto.SerialNumber);

                if (existingWithSerial != null)
                {
                    _logger.Warn($"Оборудование с серийным номером {equipmentDto.SerialNumber} уже существует");
                    return OperationResult<int>.Failure("Оборудование с таким серийным номером уже существует");
                }
            }

            // Проверка существования ответственного сотрудника
            if (equipmentDto.ResponsibleEmployee != null && equipmentDto.ResponsibleEmployee.EmployeeID > 0)
            {
                var employee = await _employeeRepository.GetByIdAsync(equipmentDto.ResponsibleEmployee.EmployeeID);
                if (employee == null)
                {
                    _logger.Warn($"Ответственный сотрудник с ID {equipmentDto.ResponsibleEmployee.EmployeeID} не найден");
                    return OperationResult<int>.Failure("Ответственный сотрудник не найден");
                }
            }

            // Проверка существования отдела
            if (equipmentDto.departmentDTO != null && equipmentDto.departmentDTO.DepartmentID > 0)
            {
                var department = await _departmentRepository.GetByIdAsync(equipmentDto.departmentDTO.DepartmentID);
                if (department == null)
                {
                    _logger.Warn($"Отдел с ID {equipmentDto.departmentDTO.DepartmentID} не найден");
                    return OperationResult<int>.Failure("Отдел не найден");
                }
            }

            // Преобразование DTO в entity
            var equipment = new Equipment
            {
                Name = equipmentDto.Name.Trim(),
                Model = equipmentDto.Model?.Trim(),
                SerialNumber = equipmentDto.SerialNumber?.Trim(),
                Type = equipmentDto.Type?.Trim(),
                Manufacturer = equipmentDto.Manufacturer?.Trim(),
                PurchaseDate = equipmentDto.PurchaseDate,
                ResponsibleEmployeeID = equipmentDto.ResponsibleEmployee.EmployeeID,
                DepartmentID = equipmentDto.departmentDTO?.DepartmentID ?? 0,
                Location = equipmentDto.Location?.Trim(),
                TechnicalDocumentation = equipmentDto.TechnicalDocumentation?.Trim()
            };

            var equipmentId = await _equipmentRepository.AddAsync(equipment);

            _logger.Info($"Оборудование '{equipmentDto.Name}' создано с ID: {equipmentId}");
            return OperationResult<int>.Success(equipmentId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при создании оборудования '{equipmentDto?.Name}'");
            return OperationResult<int>.Failure("Ошибка при создании оборудования");
        }
    }

    public async Task<OperationResult> UpdateEquipmentAsync(EquipmentDTO equipmentDto)
    {
        try
        {
            _logger.Info($"Обновление оборудования с ID: {equipmentDto.EquipmentID}");

            if (equipmentDto == null)
            {
                _logger.Warn("Попытка обновления пустого оборудования");
                return OperationResult.Failure("Данные оборудования не предоставлены");
            }

            // Валидация данных
            var validationResult = ValidateEquipmentDto(equipmentDto);
            if (!validationResult.IsSuccess)
            {
                _logger.Warn($"Ошибка валидации оборудования: {validationResult.Message}");
                return OperationResult.Failure(validationResult.Message);
            }

            // Проверка существования оборудования
            var existingEquipment = await _equipmentRepository.GetByIdAsync(equipmentDto.EquipmentID);
            if (existingEquipment == null)
            {
                _logger.Warn($"Оборудование с ID {equipmentDto.EquipmentID} не найдено");
                return OperationResult.Failure("Оборудование не найдено");
            }

            // Проверка уникальности серийного номера (если изменился)
            if (!string.IsNullOrWhiteSpace(equipmentDto.SerialNumber) &&
                existingEquipment.SerialNumber != equipmentDto.SerialNumber)
            {
                var existingWithSerial = (await _equipmentRepository.GetAllAsync())
                    .FirstOrDefault(e => e.SerialNumber == equipmentDto.SerialNumber);

                if (existingWithSerial != null)
                {
                    _logger.Warn($"Оборудование с серийным номером {equipmentDto.SerialNumber} уже существует");
                    return OperationResult.Failure("Оборудование с таким серийным номером уже существует");
                }
            }

            // Проверка существования ответственного сотрудника
            if (equipmentDto.ResponsibleEmployee != null && equipmentDto.ResponsibleEmployee.EmployeeID > 0)
            {
                var employee = await _employeeRepository.GetByIdAsync(equipmentDto.ResponsibleEmployee.EmployeeID);
                if (employee == null)
                {
                    _logger.Warn($"Ответственный сотрудник с ID {equipmentDto.ResponsibleEmployee.EmployeeID} не найден");
                    return OperationResult.Failure("Ответственный сотрудник не найден");
                }
            }

            // Проверка существования отдела
            if (equipmentDto.departmentDTO != null && equipmentDto.departmentDTO.DepartmentID > 0)
            {
                var department = await _departmentRepository.GetByIdAsync(equipmentDto.departmentDTO.DepartmentID);
                if (department == null)
                {
                    _logger.Warn($"Отдел с ID {equipmentDto.departmentDTO.DepartmentID} не найден");
                    return OperationResult.Failure("Отдел не найден");
                }
            }

            // Обновление данных
            existingEquipment.Name = equipmentDto.Name.Trim();
            existingEquipment.Model = equipmentDto.Model?.Trim();
            existingEquipment.SerialNumber = equipmentDto.SerialNumber?.Trim();
            existingEquipment.Type = equipmentDto.Type?.Trim();
            existingEquipment.Manufacturer = equipmentDto.Manufacturer?.Trim();
            existingEquipment.PurchaseDate = equipmentDto.PurchaseDate;
            existingEquipment.ResponsibleEmployeeID = equipmentDto.ResponsibleEmployee?.EmployeeID ?? 0;
            existingEquipment.DepartmentID = equipmentDto.departmentDTO?.DepartmentID ?? 0;
            existingEquipment.Location = equipmentDto.Location?.Trim();
            existingEquipment.TechnicalDocumentation = equipmentDto.TechnicalDocumentation?.Trim();

            var result = await _equipmentRepository.UpdateAsync(existingEquipment);

            if (result > 0)
            {
                _logger.Info($"Оборудование с ID {equipmentDto.EquipmentID} успешно обновлено");
                return OperationResult.Success();
            }
            else
            {
                _logger.Warn($"Не удалось обновить оборудование с ID {equipmentDto.EquipmentID}");
                return OperationResult.Failure("Не удалось обновить оборудование");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при обновлении оборудования с ID {equipmentDto?.EquipmentID}");
            return OperationResult.Failure("Ошибка при обновлении оборудования");
        }
    }

    public async Task<OperationResult> DeleteEquipmentAsync(int id)
    {
        try
        {
            _logger.Info($"Удаление оборудования с ID: {id}");

            if (id <= 0)
            {
                _logger.Warn("Неверный ID оборудования для удаления");
                return OperationResult.Failure("Неверный идентификатор оборудования");
            }

            var existingEquipment = await _equipmentRepository.GetByIdAsync(id);
            if (existingEquipment == null)
            {
                _logger.Warn($"Оборудование с ID {id} не найдено для удаления");
                return OperationResult.Failure("Оборудование не найдено");
            }

        
        var activeRequests = await _requestRepository.GetRequestsByEquipmentAsync(id);
            if (activeRequests.Any(r => r.RequestStatus.Name != "Completed"))
            {
                _logger.Warn($"Невозможно удалить оборудование с ID {id} - есть активные заявки");
                return OperationResult.Failure("Невозможно удалить оборудование - есть активные заявки");
            }

            var result = await _equipmentRepository.DeleteAsync(id);

            if (result > 0)
            {
                _logger.Info($"Оборудование с ID {id} успешно удалено");
                return OperationResult.Success();
            }
            else
            {
                _logger.Warn($"Не удалось удалить оборудование с ID {id}");
                return OperationResult.Failure("Не удалось удалить оборудование");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при удалении оборудования с ID {id}");
            return OperationResult.Failure("Ошибка при удалении оборудования");
        }
    }


    private OperationResult ValidateEquipmentDto(EquipmentDTO equipmentDto)
    {
        if (string.IsNullOrWhiteSpace(equipmentDto.Name))
            return OperationResult.Failure("Название оборудования обязательно");

        if (equipmentDto.Name.Length > 100)
            return OperationResult.Failure("Название оборудования слишком длинное");

        if (!string.IsNullOrWhiteSpace(equipmentDto.Model) && equipmentDto.Model.Length > 50)
            return OperationResult.Failure("Модель оборудования слишком длинная");

        if (!string.IsNullOrWhiteSpace(equipmentDto.SerialNumber) && equipmentDto.SerialNumber.Length > 50)
            return OperationResult.Failure("Серийный номер слишком длинный");

        if (!string.IsNullOrWhiteSpace(equipmentDto.Type) && equipmentDto.Type.Length > 50)
            return OperationResult.Failure("Тип оборудования слишком длинный");

        if (!string.IsNullOrWhiteSpace(equipmentDto.Manufacturer) && equipmentDto.Manufacturer.Length > 100)
            return OperationResult.Failure("Производитель слишком длинный");

        if (!string.IsNullOrWhiteSpace(equipmentDto.Location) && equipmentDto.Location.Length > 200)
            return OperationResult.Failure("Местоположение слишком длинное");

        if (equipmentDto.PurchaseDate > DateTime.Now)
            return OperationResult.Failure("Дата покупки не может быть в будущем");

        return OperationResult.Success();
    }
}