using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;

namespace TaskTamer_Application.Service
{
    public class PositionService
    {


        private readonly IPositionRepository _positionRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public PositionService(IPositionRepository positionRepository, IEmployeeRepository employeeRepository)
        {
            _positionRepository = positionRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<OperationResult<int>> CreatePositionAsync(PositionDTO positionDTO)
        {
            try
            {
                _logger.Info($"Создание должности: {positionDTO.Title}");
                if (positionDTO == null)
                {
                    _logger.Warn("Попытка создания пустой должности");
                    return OperationResult<int>.Failure("Данные роли не предоставлены");
                }

                var validationResult = ValidatePositionDto(positionDTO);
                if (!validationResult.IsSuccess)
                {
                    _logger.Warn($"Ошибка валидации: {validationResult.Message}");
                    return OperationResult<int>.Failure(validationResult.Message);
                }

                var existingRole = await _positionRepository.GetByNameAsync(positionDTO.Title);
                if (existingRole != null)
                {
                    _logger.Warn($"Должность с именем '{positionDTO.Title}' уже существует");
                    return OperationResult<int>.Failure("Должность с таким именем уже существует");
                }
                var position = new Position
                {
                    Title = positionDTO.Title.Trim(),
                    Description = positionDTO.Description?.Trim(),
                    AccessLevel = positionDTO.AccessLevel
                };

                var positionId = await _positionRepository.AddAsync(position);

                _logger.Info($"Должность '{positionDTO.Title}' создана с ID: {positionId}");
                return OperationResult<int>.Success(positionId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при создании должности '{positionDTO?.Title}'");
                return OperationResult<int>.Failure("Ошибка при создании должности");
            }
        }

        public async Task<OperationResult<PositionDTO>> GetPositionByNameAsync(string name)
        {
            try
            {
                _logger.Debug($"Запрос должности по имени: {name}");
                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.Warn("Неверное имя должности для запроса");
                    return OperationResult<PositionDTO>.Failure("Имя должности не указано");
                }
                var pos = await _positionRepository.GetByNameAsync(name.Trim());
                if (pos == null)
                {
                    _logger.Warn($"Должность с именем '{name}' не найдена");
                    return OperationResult<PositionDTO>.Failure("Должность не найдена");
                }
                var posDto = new PositionDTO(pos);
                _logger.Debug($"Должность с именем '{name}' успешно получена");
                return OperationResult<PositionDTO>.Success(posDto);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при получении должности с именем '{name}'");
                return OperationResult<PositionDTO>.Failure("Ошибка при получении должности");
            }
        }

        public async Task<OperationResult<IEnumerable<PositionDTO>>> GetAllPositionsAsync()
        {
            try
            {
                _logger.Debug("Запрос всех должностей");

                var positions = await _positionRepository.GetAllAsync();

                if (positions == null || !positions.Any())
                {
                    _logger.Info("Должности не найдены");
                    return OperationResult<IEnumerable<PositionDTO>>.Success(Enumerable.Empty<PositionDTO>());
                }

                var posDtos = positions.Select(p => new PositionDTO(p)).ToList();
                _logger.Debug($"Получено {posDtos.Count} должностей");
                return OperationResult<IEnumerable<PositionDTO>>.Success(posDtos);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при получении списка должностей");
                return OperationResult<IEnumerable<PositionDTO>>.Failure("Ошибка при получении списка должностей");
            }
        }

        public async Task<OperationResult> UpdatePositionAsync(PositionDTO posDto)
        {
            try
            {
                _logger.Info($"Обновление должности с ID: {posDto.PositionID}");
                if (posDto == null)
                {
                    _logger.Warn("Попытка обновления пустой должности");
                    return OperationResult.Failure("Данные должности не предоставлены");
                }

                var validationResult = ValidatePositionDto(posDto);
                if (!validationResult.IsSuccess)
                {
                    _logger.Warn($"Ошибка валидации: {validationResult.Message}");
                    return OperationResult.Failure(validationResult.Message);
                }

                var existingPos = await _positionRepository.GetByIdAsync(posDto.PositionID);
                if (existingPos == null)
                {
                    _logger.Warn($"Должность с ID {posDto.PositionID} не найдена");
                    return OperationResult.Failure("Должность не найдена");
                }

                if (!string.Equals(existingPos.Title, posDto.Title, StringComparison.OrdinalIgnoreCase))
                {
                    var posWithSameName = await _positionRepository.GetByNameAsync(posDto.Title);
                    if (posWithSameName != null)
                    {
                        _logger.Warn($"Должность с именем '{posDto.Title}' уже существует");
                        return OperationResult.Failure("Должность с таким именем уже существует");
                    }
                }

                existingPos.Title = posDto.Title.Trim();
                existingPos.Description = posDto.Description?.Trim();
                existingPos.AccessLevel = posDto.AccessLevel;

                var result = await _positionRepository.UpdateAsync(existingPos);
                if (result > 0)
                {
                    _logger.Info($"Должность с ID {posDto.PositionID} успешно обновлена");
                    return OperationResult.Success();
                }
                else
                {
                    _logger.Warn($"Не удалось обновить должность с ID {posDto.PositionID}");
                    return OperationResult.Failure("Не удалось обновить должность");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при обновлении должности с ID {posDto?.PositionID}");
                return OperationResult.Failure("Ошибка при обновлении должности");
            }
        }
        public async Task<OperationResult> DeletePositionAsync(int id)
        {
            try
            {
                _logger.Info($"Удаление должности с ID: {id}");
                if (id <= 0)
                {
                    _logger.Warn("Неверный ID должности для удаления");
                    return OperationResult.Failure("Неверный идентификатор должности");
                }

                var existingPos = await _positionRepository.GetByIdAsync(id);
                if (existingPos == null)
                {
                    _logger.Warn($"Должность с ID {id} не найдена для удаления");
                    return OperationResult.Failure("Должность не найдена");
                }
                var empWithPos = await _employeeRepository.GetEmployeeWithPositionAsync(id);
                if (empWithPos.Any())
                {
                    _logger.Warn($"Невозможно удалить должность с ID {id} - она используется пользователями");
                    return OperationResult.Failure("Невозможно удалить должность - она используется пользователями");
                }

                var result = await _positionRepository.DeleteAsync(id);
                if (result > 0)
                {
                    _logger.Info($"Должность с ID {id} успешно удалена");
                    return OperationResult.Success();
                }
                else
                {
                    _logger.Warn($"Не удалось удалить должность с ID {id}");
                    return OperationResult.Failure("Не удалось удалить должность");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при удалении должности с ID {id}");
                return OperationResult.Failure("Ошибка при удалении должности");
            }
        }
        public async Task<OperationResult<PositionDTO>> GetPositionByIdAsync(int id)
        {
            try
            {
                _logger.Debug($"Запрос должности с ID: {id}");
                if (id <= 0)
                {
                    _logger.Warn("Неверный ID должности для запроса");
                    return OperationResult<PositionDTO>.Failure("Неверный идентификатор должности");
                }
                var pos = await _positionRepository.GetByIdAsync(id);
                if (pos == null)
                {
                    _logger.Warn($"Должность с ID {id} не найдена");
                    return OperationResult<PositionDTO>.Failure("Должность не найдена");
                }
                var posDto = new PositionDTO(pos);
                _logger.Debug($"Должность с ID {id} успешно получена");
                return OperationResult<PositionDTO>.Success(posDto);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при получении должности с ID {id}");
                return OperationResult<PositionDTO>.Failure("Ошибка при получении должности");
            }
        }
        private OperationResult ValidatePositionDto(PositionDTO posDto)
        {
            if (string.IsNullOrWhiteSpace(posDto.Title))
                return OperationResult.Failure("Название должности обязательно");

            if (posDto.Title.Length > 50)
                return OperationResult.Failure("Название должности слишком длинное");

            if (!string.IsNullOrWhiteSpace(posDto.Description) && posDto.Description.Length > 200)
                return OperationResult.Failure("Описание должности слишком длинное");

            if (posDto.AccessLevel < 1 || posDto.AccessLevel > 10)
                return OperationResult.Failure("Уровень доступа должен быть от 1 до 10");

            return OperationResult.Success();
        }
    }

}
