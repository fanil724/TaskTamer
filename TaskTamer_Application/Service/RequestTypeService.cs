using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;

namespace TaskTamer_Application.Service
{
    public class RequestTypeService
    {

        private readonly IRequestTypeRepository _requestTypeRepository;
        private readonly IRequestRepository _requestRepository;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public RequestTypeService(IRequestTypeRepository requestTypeRepository, IRequestRepository requestRepository)
        {
            _requestTypeRepository = requestTypeRepository;
            _requestRepository = requestRepository;
        }

        public async Task<OperationResult<int>> CreateRequestTypeAsync(RequestTypeDTO requestTypeDTO)
        {
            try
            {
                _logger.Info($"Создание типа: {requestTypeDTO.Name}");
                if (requestTypeDTO == null)
                {
                    _logger.Warn("Попытка создания пустой типа");
                    return OperationResult<int>.Failure("Данные типа не предоставлены");
                }

                var validationResult = ValidateRequestTypeDto(requestTypeDTO);
                if (!validationResult.IsSuccess)
                {
                    _logger.Warn($"Ошибка валидации: {validationResult.Message}");
                    return OperationResult<int>.Failure(validationResult.Message);
                }

                var existingType = await _requestTypeRepository.GetByNameAsync(requestTypeDTO.Name);
                if (existingType != null)
                {
                    _logger.Warn($"Тип с именем '{requestTypeDTO.Name}' уже существует");
                    return OperationResult<int>.Failure("Тип с таким именем уже существует");
                }
                var type = new RequestType
                {
                    Name = requestTypeDTO.Name.Trim(),
                    Description = requestTypeDTO.Description?.Trim()??"",

                };

                var typeId = await _requestTypeRepository.AddAsync(type);

                _logger.Info($"Тип '{requestTypeDTO.Name}' создана с ID: {typeId}");
                return OperationResult<int>.Success(typeId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при создании типа '{requestTypeDTO?.Name}'");
                return OperationResult<int>.Failure("Ошибка при создании типа");
            }
        }

        public async Task<OperationResult<RequestTypeDTO>> GetRequestTypeByNameAsync(string name)
        {
            try
            {
                _logger.Debug($"Запрос типа по имени: {name}");
                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.Warn("Неверное имя типа для запроса");
                    return OperationResult<RequestTypeDTO>.Failure("Имя типа не указано");
                }
                var type = await _requestTypeRepository.GetByNameAsync(name.Trim());
                if (type == null)
                {
                    _logger.Warn($"Тип с именем '{name}' не найдена");
                    return OperationResult<RequestTypeDTO>.Failure("Тип не найдена");
                }
                var typeDto = new RequestTypeDTO(type);
                _logger.Debug($"Тип с именем '{name}' успешно получена");
                return OperationResult<RequestTypeDTO>.Success(typeDto);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при получении типа с именем '{name}'");
                return OperationResult<RequestTypeDTO>.Failure("Ошибка при получении типа");
            }
        }

        public async Task<OperationResult<IEnumerable<RequestTypeDTO>>> GetAllRequestTypesAsync()
        {
            try
            {
                _logger.Debug("Запрос всех типов");

                var types = await _requestTypeRepository.GetAllAsync();

                if (types == null || !types.Any())
                {
                    _logger.Info("Типы не найдены");
                    return OperationResult<IEnumerable<RequestTypeDTO>>.Success(Enumerable.Empty<RequestTypeDTO>());
                }

                var typesDtos = types.Select(r => new RequestTypeDTO(r)).ToList();
                _logger.Debug($"Получено {typesDtos.Count} ролей");
                return OperationResult<IEnumerable<RequestTypeDTO>>.Success(typesDtos);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при получении списка типов");
                return OperationResult<IEnumerable<RequestTypeDTO>>.Failure("Ошибка при получении списка типов");
            }
        }

        public async Task<OperationResult> UpdateRequestTypeAsync(RequestTypeDTO requestTypeDTO)
        {
            try
            {
                _logger.Info($"Обновление типа с ID: {requestTypeDTO.RequestTypeID}");
                if (requestTypeDTO == null)
                {
                    _logger.Warn("Попытка обновления пустой типа");
                    return OperationResult.Failure("Данные типа не предоставлены");
                }

                var validationResult = ValidateRequestTypeDto(requestTypeDTO);
                if (!validationResult.IsSuccess)
                {
                    _logger.Warn($"Ошибка валидации: {validationResult.Message}");
                    return OperationResult.Failure(validationResult.Message);
                }

                var existingType = await _requestTypeRepository.GetByIdAsync(requestTypeDTO.RequestTypeID);
                if (existingType == null)
                {
                    _logger.Warn($"Тип с ID {requestTypeDTO.RequestTypeID} не найдена");
                    return OperationResult.Failure("Тип не найдена");
                }

                if (!string.Equals(existingType.Name, requestTypeDTO.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var roleWithSameName = await _requestTypeRepository.GetByNameAsync(requestTypeDTO.Name);
                    if (roleWithSameName != null)
                    {
                        _logger.Warn($"Тип с именем '{requestTypeDTO.Name}' уже существует");
                        return OperationResult.Failure("Тип с таким именем уже существует");
                    }
                }

                existingType.Name = requestTypeDTO.Name.Trim();
                existingType.Description = requestTypeDTO.Description?.Trim()??"";


                var result = await _requestTypeRepository.UpdateAsync(existingType);
                if (result > 0)
                {
                    _logger.Info($"Тип с ID {requestTypeDTO.RequestTypeID} успешно обновлена");
                    return OperationResult.Success();
                }
                else
                {
                    _logger.Warn($"Не удалось обновить тип с ID {requestTypeDTO.RequestTypeID}");
                    return OperationResult.Failure("Не удалось обновить тип");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при обновлении типа с ID {requestTypeDTO?.RequestTypeID}");
                return OperationResult.Failure("Ошибка при обновлении типа");
            }
        }
        public async Task<OperationResult> DeleteRequestTypeAsync(int id)
        {
            try
            {
                _logger.Info($"Удаление типа с ID: {id}");
                if (id <= 0)
                {
                    _logger.Warn("Неверный ID тип запроса для удаления");
                    return OperationResult.Failure("Неверный идентификатор тип запроса");
                }

                var existingType = await _requestTypeRepository.GetByIdAsync(id);
                if (existingType == null)
                {
                    _logger.Warn($"Тип запроса с ID {id} не найден для удаления");
                    return OperationResult.Failure("Тип запроса не найден");
                }
                var reqWithRequestType = await _requestRepository.GetRequestWithRequestTypeAsync(id);
                if (reqWithRequestType.Any())
                {
                    _logger.Warn($"Невозможно удалить тип запроса с ID {id} - она используется в заявке");
                    return OperationResult.Failure("Невозможно удалить тип запроса - она используется в заявке");
                }

                var result = await _requestTypeRepository.DeleteAsync(id);
                if (result > 0)
                {
                    _logger.Info($"Тип с ID {id} успешно удалена");
                    return OperationResult.Success();
                }
                else
                {
                    _logger.Warn($"Не удалось удалить роль с ID {id}");
                    return OperationResult.Failure("Не удалось удалить тип запроса");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при удалении типа с ID {id}");
                return OperationResult.Failure("Ошибка при удалении типа");
            }
        }
        public async Task<OperationResult<RequestTypeDTO>> GetRequestTypeByIdAsync(int id)
        {
            try
            {
                _logger.Debug($"Запрос типа с ID: {id}");
                if (id <= 0)
                {
                    _logger.Warn("Неверный ID типа для запроса");
                    return OperationResult<RequestTypeDTO>.Failure("Неверный идентификатор типа");
                }
                var type = await _requestTypeRepository.GetByIdAsync(id);
                if (type == null)
                {
                    _logger.Warn($"Тип с ID {id} не найдена");
                    return OperationResult<RequestTypeDTO>.Failure("Тип не найдена");
                }
                var typeDto = new RequestTypeDTO(type);
                _logger.Debug($"Тип с ID {id} успешно получена");
                return OperationResult<RequestTypeDTO>.Success(typeDto);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при получении типа с ID {id}");
                return OperationResult<RequestTypeDTO>.Failure("Ошибка при получении типа");
            }
        }
        private OperationResult ValidateRequestTypeDto(RequestTypeDTO requestTypeDTO)
        {
            if (string.IsNullOrWhiteSpace(requestTypeDTO.Name))
                return OperationResult.Failure("Название типа обязательно");

            if (requestTypeDTO.Name.Length > 50)
                return OperationResult.Failure("Название типа слишком длинное");

            if (!string.IsNullOrWhiteSpace(requestTypeDTO.Description) && requestTypeDTO.Description.Length > 200)
                return OperationResult.Failure("Описание типа слишком длинное");

            return OperationResult.Success();
        }
    }
}
