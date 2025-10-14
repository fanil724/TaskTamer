using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;

namespace TaskTamer_Application.Service
{
    public class RequestStatusService
    {
        private readonly IRequestStatusRepository _requestStatusRepository;
        private readonly IRequestRepository _requestRepository;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public RequestStatusService(IRequestStatusRepository requestStatusRepository, IRequestRepository requestRepository)
        {
            _requestStatusRepository = requestStatusRepository;
            _requestRepository = requestRepository;
        }

        public async Task<OperationResult<int>> CreateRequestStatusAsync(RequestStatusDTO requestStatusDTO)
        {
            try
            {
                _logger.Info($"Создание статуса: {requestStatusDTO.Name}");
                if (requestStatusDTO == null)
                {
                    _logger.Warn("Попытка создания пустой статуса");
                    return OperationResult<int>.Failure("Данные статуса не предоставлены");
                }

                var validationResult = ValidateRequestStatusDto(requestStatusDTO);
                if (!validationResult.IsSuccess)
                {
                    _logger.Warn($"Ошибка валидации: {validationResult.Message}");
                    return OperationResult<int>.Failure(validationResult.Message);
                }

                var existingStatus = await _requestStatusRepository.GetByNameAsync(requestStatusDTO.Name);
                if (existingStatus != null)
                {
                    _logger.Warn($"Статус с именем '{requestStatusDTO.Name}' уже существует");
                    return OperationResult<int>.Failure("Статус с таким именем уже существует");
                }
                var status = new RequestStatus
                {
                    Name = requestStatusDTO.Name.Trim(),
                    Description = requestStatusDTO.Description?.Trim() ?? "",

                };

                var statusId = await _requestStatusRepository.AddAsync(status);

                _logger.Info($"Статус '{requestStatusDTO.Name}' создана с ID: {statusId}");
                return OperationResult<int>.Success(statusId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при создании статуса '{requestStatusDTO?.Name}'");
                return OperationResult<int>.Failure("Ошибка при создании статуса");
            }
        }

        public async Task<OperationResult<RequestStatusDTO>> GetRequestStatusByNameAsync(string name)
        {
            try
            {
                _logger.Debug($"Запрос статуса по имени: {name}");
                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.Warn("Неверное имя статуса для запроса");
                    return OperationResult<RequestStatusDTO>.Failure("Имя статуса не указано");
                }
                var status = await _requestStatusRepository.GetByNameAsync(name.Trim());
                if (status == null)
                {
                    _logger.Warn($"Статус с именем '{name}' не найдена");
                    return OperationResult<RequestStatusDTO>.Failure("Тип не найдена");
                }
                var statusDto = new RequestStatusDTO(status);
                _logger.Debug($"Статус с именем '{name}' успешно получен");
                return OperationResult<RequestStatusDTO>.Success(statusDto);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при получении статуса с именем '{name}'");
                return OperationResult<RequestStatusDTO>.Failure("Ошибка при получении статуса");
            }
        }

        public async Task<OperationResult<IEnumerable<RequestStatusDTO>>> GetAllRequestStatusAsync()
        {
            try
            {
                _logger.Debug("Запрос всех статусов");

                var status = await _requestStatusRepository.GetAllAsync();

                if (status == null || !status.Any())
                {
                    _logger.Info("Статусы не найдены");
                    return OperationResult<IEnumerable<RequestStatusDTO>>.Success(Enumerable.Empty<RequestStatusDTO>());
                }

                var statusDtos = status.Select(r => new RequestStatusDTO(r)).ToList();
                _logger.Debug($"Получено {statusDtos.Count} статусов");
                return OperationResult<IEnumerable<RequestStatusDTO>>.Success(statusDtos);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при получении списка статусов");
                return OperationResult<IEnumerable<RequestStatusDTO>>.Failure("Ошибка при получении списка статусов");
            }
        }

        public async Task<OperationResult> UpdateRequestStatusAsync(RequestStatusDTO requestStatusDto)
        {
            try
            {
                _logger.Info($"Обновление статуса с ID: {requestStatusDto.StatusID}");
                if (requestStatusDto == null)
                {
                    _logger.Warn("Попытка обновления пустой статуса");
                    return OperationResult.Failure("Данные статуса не предоставлены");
                }

                var validationResult = ValidateRequestStatusDto(requestStatusDto);
                if (!validationResult.IsSuccess)
                {
                    _logger.Warn($"Ошибка валидации: {validationResult.Message}");
                    return OperationResult.Failure(validationResult.Message);
                }

                var existingStatus = await _requestStatusRepository.GetByIdAsync(requestStatusDto.StatusID);
                if (existingStatus == null)
                {
                    _logger.Warn($"Статус с ID {requestStatusDto.StatusID} не найдена");
                    return OperationResult.Failure("Статус не найдена");
                }

                if (!string.Equals(existingStatus.Name, requestStatusDto.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var roleWithSameName = await _requestStatusRepository.GetByNameAsync(requestStatusDto.Name);
                    if (roleWithSameName != null)
                    {
                        _logger.Warn($"Статус с именем '{requestStatusDto.Name}' уже существует");
                        return OperationResult.Failure("Статус с таким именем уже существует");
                    }
                }

                existingStatus.Name = requestStatusDto.Name.Trim();
                existingStatus.Description = requestStatusDto.Description?.Trim() ?? "";


                var result = await _requestStatusRepository.UpdateAsync(existingStatus);
                if (result > 0)
                {
                    _logger.Info($"Статус с ID {requestStatusDto.StatusID} успешно обновлена");
                    return OperationResult.Success();
                }
                else
                {
                    _logger.Warn($"Не удалось обновить статус с ID {requestStatusDto.StatusID}");
                    return OperationResult.Failure("Не удалось обновить статус");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при обновлении статуса с ID {requestStatusDto?.StatusID}");
                return OperationResult.Failure("Ошибка при обновлении статуса");
            }
        }
        public async Task<OperationResult> DeleteRequestStatusAsync(int id)
        {
            try
            {
                _logger.Info($"Удаление статуса с ID: {id}");
                if (id <= 0)
                {
                    _logger.Warn("Неверный ID статуса запроса для удаления");
                    return OperationResult.Failure("Неверный идентификатор статуса запроса");
                }

                var existingStatus = await _requestStatusRepository.GetByIdAsync(id);
                if (existingStatus == null)
                {
                    _logger.Warn($"Статус запроса с ID {id} не найден для удаления");
                    return OperationResult.Failure("Статус запроса не найден");
                }
                var reqWithRequestStatus = await _requestRepository.GetRequestWithRequestTypeAsync(id);
                if (reqWithRequestStatus.Any())
                {
                    _logger.Warn($"Невозможно удалить статус запроса с ID {id} - она используется в заявке");
                    return OperationResult.Failure("Невозможно удалить статус запроса - она используется в заявке");
                }

                var result = await _requestStatusRepository.DeleteAsync(id);
                if (result > 0)
                {
                    _logger.Info($"Статус с ID {id} успешно удалена");
                    return OperationResult.Success();
                }
                else
                {
                    _logger.Warn($"Не удалось удалить статус с ID {id}");
                    return OperationResult.Failure("Не удалось удалить статус запроса");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при удалении статуса с ID {id}");
                return OperationResult.Failure("Ошибка при удалении статуса");
            }
        }
        public async Task<OperationResult<RequestStatusDTO>> GetRequestStatusByIdAsync(int id)
        {
            try
            {
                _logger.Debug($"Запрос статуса с ID: {id}");
                if (id <= 0)
                {
                    _logger.Warn("Неверный ID статуса для запроса");
                    return OperationResult<RequestStatusDTO>.Failure("Неверный идентификатор статуса");
                }
                var status = await _requestStatusRepository.GetByIdAsync(id);
                if (status == null)
                {
                    _logger.Warn($"Статус с ID {id} не найдена");
                    return OperationResult<RequestStatusDTO>.Failure("Статус не найдена");
                }
                var statusDto = new RequestStatusDTO(status);
                _logger.Debug($"Статус с ID {id} успешно получена");
                return OperationResult<RequestStatusDTO>.Success(statusDto);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при получении статуса с ID {id}");
                return OperationResult<RequestStatusDTO>.Failure("Ошибка при получении статуса");
            }
        }
        private OperationResult ValidateRequestStatusDto(RequestStatusDTO requestStatusDTO)
        {
            if (string.IsNullOrWhiteSpace(requestStatusDTO.Name))
                return OperationResult.Failure("Название статуса обязательно");

            if (requestStatusDTO.Name.Length > 50)
                return OperationResult.Failure("Название статуса слишком длинное");

            if (!string.IsNullOrWhiteSpace(requestStatusDTO.Description) && requestStatusDTO.Description.Length > 200)
                return OperationResult.Failure("Описание статуса слишком длинное");

            return OperationResult.Success();
        }
    }
}

