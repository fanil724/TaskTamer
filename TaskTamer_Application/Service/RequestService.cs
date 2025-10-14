using NLog;
using System.Text;
using TaskTamer_Application.Contracts;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;

namespace TaskTamer_Application.Service;

public class RequestService
{
    private readonly IRequestRepository _requestRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IRequestStatusRepository _statusRepository;
    private readonly IRequestTypeRepository _typeRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IRequestHistoryRepository _requestHistoryRepository;
    private readonly IUserRepository _userRepository;

    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public RequestService(
        IRequestRepository requestRepository,
        IEmployeeRepository employeeRepository,
        IRequestStatusRepository statusRepository,
        IRequestTypeRepository typeRepository,
        IEquipmentRepository equipmentRepository,
        IRequestHistoryRepository requestHistoryRepository,
        IUserRepository userRepository)
    {
        _requestRepository = requestRepository;
        _employeeRepository = employeeRepository;
        _statusRepository = statusRepository;
        _typeRepository = typeRepository;
        _equipmentRepository = equipmentRepository;
        _requestHistoryRepository = requestHistoryRepository;
        _userRepository = userRepository;
    }

    public async Task<OperationResult<RequestDTO>> GetRequestByIdAsync(int id)
    {
        try
        {
            _logger.Debug($"Запрос заявки с ID: {id}");

            if (id <= 0)
            {
                _logger.Warn("Неверный ID заявки для запроса");
                return OperationResult<RequestDTO>.Failure("Неверный идентификатор заявки");
            }

            var request = await _requestRepository.GetByIdAsync(id);

            if (request == null)
            {
                _logger.Warn($"Заявка с ID {id} не найдена");
                return OperationResult<RequestDTO>.Failure("Заявка не найдена");
            }

            var requestDto = new RequestDTO(request);
            _logger.Debug($"Заявка с ID {id} успешно получена");
            return OperationResult<RequestDTO>.Success(requestDto);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при получении заявки с ID {id}");
            return OperationResult<RequestDTO>.Failure("Ошибка при получении заявки");
        }
    }

    public async Task<OperationResult<IEnumerable<RequestDTO>>> GetAllRequestsAsync()
    {
        try
        {
            _logger.Debug("Запрос всех заявок");

            var requests = await _requestRepository.GetAllAsync();

            if (requests == null || !requests.Any())
            {
                _logger.Info("Заявки не найдены");
                return OperationResult<IEnumerable<RequestDTO>>.Success(Enumerable.Empty<RequestDTO>());
            }

            var requestDtos = requests.Select(r => new RequestDTO(r)).ToList();
            _logger.Debug($"Получено {requestDtos.Count} заявок");
            return OperationResult<IEnumerable<RequestDTO>>.Success(requestDtos);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при получении списка заявок");
            return OperationResult<IEnumerable<RequestDTO>>.Failure("Ошибка при получении списка заявок");
        }
    }

    public async Task<OperationResult<int>> CreateRequestAsync(RequestDTO requestDto)
    {
        try
        {          

            if (requestDto == null)
            {
                _logger.Warn("Попытка создания пустой заявки");
                return OperationResult<int>.Failure("Данные заявки не предоставлены");
            }
                       
            _logger.Info($"Создание новой заявки от сотрудника ID: {requestDto.Author?.EmployeeID}");
            
            // Валидация данных
            var validationResult = ValidateRequestDto(requestDto);
            if (!validationResult.IsSuccess)
            {
                _logger.Warn($"Ошибка валидации заявки: {validationResult.Message}");
                return OperationResult<int>.Failure(validationResult.Message);
            }

            // Проверка существования связанных сущностей
            var author = await _employeeRepository.GetByIdAsync(requestDto.Author.EmployeeID);
            if (author == null)
            {
                _logger.Warn($"Автор с ID {requestDto.Author.EmployeeID} не найден");
                return OperationResult<int>.Failure("Автор заявки не найден");
            }

            var status = await _statusRepository.GetByIdAsync(requestDto.RequestStatus.StatusID);
            if (status == null)
            {
                _logger.Warn($"Статус с ID {requestDto.RequestStatus.StatusID} не найден");
                return OperationResult<int>.Failure("Статус заявки не найден");
            }

            var type = await _typeRepository.GetByIdAsync(requestDto.RequestType.RequestTypeID);
            if (type == null)
            {
                _logger.Warn($"Тип заявки с ID {requestDto.RequestType.RequestTypeID} не найден");
                return OperationResult<int>.Failure("Тип заявки не найден");
            }

            if (requestDto.Equipment != null)
            {
                var equipment = await _equipmentRepository.GetByIdAsync(requestDto.Equipment.EquipmentID);
                if (equipment == null)
                {
                    _logger.Warn($"Оборудование с ID {requestDto.Equipment.EquipmentID} не найдено");
                    return OperationResult<int>.Failure("Оборудование не найдено");
                }
            }

            var request = new Request
            {
                CreationDate = DateTime.UtcNow,
                AuthorID = requestDto.Author.EmployeeID,
                RequestStatusID = requestDto.RequestStatus.StatusID,
                RequestTypeID = requestDto.RequestType.RequestTypeID,
                ProblemDescription = requestDto.ProblemDescription.Trim(),
                Priority = requestDto.Priority,
                EquipmentID = requestDto.Equipment?.EquipmentID,
                ExecutorID = requestDto.Executor?.EmployeeID,
                Deadline = requestDto.Deadline,
                CompletionDate = null
            };

            var requestId = await _requestRepository.AddAsync(request);

            // Добавление записи в историю

            await AddHistoryRecord(
                requestId, request.RequestStatusID,
                request.ExecutorID ?? 0, $"Заявка создана с ID: {requestId}"
                );

            _logger.Info($"Заявка создана с ID: {requestId}");
            return OperationResult<int>.Success(requestId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при создании заявки");
            return OperationResult<int>.Failure("Ошибка при создании заявки");
        }
    }


    public async Task<OperationResult> UpdateRequestAsync(RequestDTO requestDto, int userId)
    {
        try
        {
            _logger.Info($"Обновление заявки с ID: {requestDto.RequestID}");

            if (requestDto == null)
            {
                _logger.Warn("Попытка обновления пустой заявки");
                return OperationResult.Failure("Данные заявки не предоставлены");
            }

            var validationResult = ValidateRequestDto(requestDto);
            if (!validationResult.IsSuccess)
            {
                _logger.Warn($"Ошибка валидации заявки: {validationResult.Message}");
                return OperationResult.Failure(validationResult.Message);
            }

            var existingRequest = await _requestRepository.GetByIdAsync(requestDto.RequestID);
            if (existingRequest == null)
            {
                _logger.Warn($"Заявка с ID {requestDto.RequestID} не найдена");
                return OperationResult.Failure("Заявка не найдена");
            }

            var status = await _statusRepository.GetByIdAsync(requestDto.RequestStatus.StatusID);
            if (status == null)
            {
                _logger.Warn($"Статус с ID {requestDto.RequestStatus.StatusID} не найден");
                return OperationResult.Failure("Статус заявки не найден");
            }

            var type = await _typeRepository.GetByIdAsync(requestDto.RequestType.RequestTypeID);
            if (type == null)
            {
                _logger.Warn($"Тип заявки с ID {requestDto.RequestType.RequestTypeID} не найден");
                return OperationResult.Failure("Тип заявки не найден");
            }

            if (requestDto.Equipment != null)
            {
                var equipment = await _equipmentRepository.GetByIdAsync(requestDto.Equipment.EquipmentID);
                if (equipment == null)
                {
                    _logger.Warn($"Оборудование с ID {requestDto.Equipment.EquipmentID} не найдено");
                    return OperationResult.Failure("Оборудование не найдено");
                }
            }


            var str = "";

            str = getUpdateComment(existingRequest, requestDto);

            if (string.IsNullOrEmpty(str))
            {
                return OperationResult.Failure("Нет изменений в заявке");
            }


            var result = await _requestRepository.UpdateAsync(existingRequest);

            if (result > 0)
            {
                var us = _userRepository.GetByIdAsync(userId).Result;
                _logger.Info($"Заявка с ID {requestDto.RequestID} успешно обновлена");
                await AddHistoryRecord(existingRequest.RequestID, existingRequest.RequestStatusID,
                    us?.EmployeeID ?? 0, str);

                return OperationResult.Success();
            }
            else
            {
                _logger.Warn($"Не удалось обновить заявку с ID {requestDto.RequestID}");
                return OperationResult.Failure("Не удалось обновить заявку");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при обновлении заявки с ID {requestDto?.RequestID}");
            return OperationResult.Failure("Ошибка при обновлении заявки");
        }
    }

    public async Task<OperationResult> DeleteRequestAsync(int id)
    {
        try
        {
            _logger.Info($"Удаление заявки с ID: {id}");

            if (id <= 0)
            {
                _logger.Warn("Неверный ID заявки для удаления");
                return OperationResult.Failure("Неверный идентификатор заявки");
            }

            var existingRequest = await _requestRepository.GetByIdAsync(id);
            if (existingRequest == null)
            {
                _logger.Warn($"Заявка с ID {id} не найдена для удаления");
                return OperationResult.Failure("Заявка не найдена");
            }

            var result = await _requestRepository.DeleteAsync(id);

            if (result > 0)
            {
                _logger.Info($"Заявка с ID {id} успешно удалена");
                return OperationResult.Success();
            }
            else
            {
                _logger.Warn($"Не удалось удалить заявку с ID {id}");
                return OperationResult.Failure("Не удалось удалить заявку");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при удалении заявки с ID {id}");
            return OperationResult.Failure("Ошибка при удалении заявки");
        }
    }

    public async Task<OperationResult<IEnumerable<RequestDTO>>> GetRequestsByStatusAsync(int statusId)
    {
        try
        {
            _logger.Debug($"Запрос заявок по статусу ID: {statusId}");

            var requests = await _requestRepository.GetByStatusWithDetailsAsync(statusId);
            var requestDtos = requests.Select(r => new RequestDTO(r)).ToList();

            _logger.Debug($"Получено {requestDtos.Count} заявок со статусом ID {statusId}");
            return OperationResult<IEnumerable<RequestDTO>>.Success(requestDtos);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при получении заявок по статусу ID {statusId}");
            return OperationResult<IEnumerable<RequestDTO>>.Failure("Ошибка при получении заявок");
        }
    }

    public async Task<OperationResult<IEnumerable<RequestDTO>>> GetRequestsByAuthorAsync(int authorId)
    {
        try
        {
            _logger.Debug($"Запрос заявок по автору ID: {authorId}");

            var requests = await _requestRepository.GetByAuthorWithDetailsAsync(authorId);
            var requestDtos = requests.Select(r => new RequestDTO(r)).ToList();

            _logger.Debug($"Получено {requestDtos.Count} заявок от автора ID {authorId}");
            return OperationResult<IEnumerable<RequestDTO>>.Success(requestDtos);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при получении заявок по автору ID {authorId}");
            return OperationResult<IEnumerable<RequestDTO>>.Failure("Ошибка при получении заявок");
        }
    }

    public async Task<OperationResult<IEnumerable<RequestDTO>>> GetRequestsByExecutorAsync(int executorId)
    {
        try
        {
            _logger.Debug($"Запрос заявок по исполнителю ID: {executorId}");

            var requests = await _requestRepository.GetByExecutorWithDetailsAsync(executorId);
            var requestDtos = requests.Select(r => new RequestDTO(r)).ToList();

            _logger.Debug($"Получено {requestDtos.Count} заявок для исполнителя ID {executorId}");
            return OperationResult<IEnumerable<RequestDTO>>.Success(requestDtos);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при получении заявок по исполнителю ID {executorId}");
            return OperationResult<IEnumerable<RequestDTO>>.Failure("Ошибка при получении заявок");
        }
    }


    private OperationResult ValidateRequestDto(RequestDTO requestDto)
    {
        if (requestDto.Author == null || requestDto.Author.EmployeeID <= 0)
            return OperationResult.Failure("Автор заявки обязателен");

        if (requestDto.RequestStatus == null || requestDto.RequestStatus.StatusID <= 0)
            return OperationResult.Failure("Статус заявки обязателен");

        if (requestDto.RequestType == null || requestDto.RequestType.RequestTypeID <= 0)
            return OperationResult.Failure("Тип заявки обязателен");

        if (string.IsNullOrWhiteSpace(requestDto.ProblemDescription))
            return OperationResult.Failure("Описание проблемы обязательно");

        if (requestDto.ProblemDescription.Length > 1000)
            return OperationResult.Failure("Описание проблемы слишком длинное");

        if (requestDto.Priority < 1 || requestDto.Priority > 5)
            return OperationResult.Failure("Приоритет должен быть от 1 до 5");

        return OperationResult.Success();
    }

    private async Task AddHistoryRecord(int requestId, int statusId, int employeeId, string comment)
    {
        try
        {

            await _requestHistoryRepository.AddAsync(new RequestHistory()
            {
                RequestID = requestId,
                ChangeDate = DateTime.UtcNow,
                RequestStatusID = statusId,
                Comment = comment,
                ChangedByID = employeeId

            });

        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при добавлении записи в историю заявки {requestId}");
        }
    }


    private string getUpdateComment(Request req, RequestDTO requestDTO)
    {
        StringBuilder commentBuilder = new StringBuilder();

        if (req.RequestStatusID != requestDTO.RequestStatus.StatusID)
        {
            var status = _statusRepository.GetByIdAsync(requestDTO.RequestStatus.StatusID).Result;
            commentBuilder.AppendLine($"Статус изменен с {req.RequestStatus.Name} на {status.Name}; ");
            req.RequestStatusID = requestDTO.RequestStatus.StatusID;
        }

        if (req.RequestTypeID != requestDTO.RequestType.RequestTypeID)
        {
            var type = _typeRepository.GetByIdAsync(requestDTO.RequestType.RequestTypeID).Result;
            commentBuilder.AppendLine($"Тип заявки изменен с {req.RequestType.Name} на {type.Name}; ");
            req.RequestTypeID = requestDTO.RequestType.RequestTypeID;
        }
        if (req.ProblemDescription?.Trim() != requestDTO.ProblemDescription?.Trim())
        {
            commentBuilder.AppendLine("Описание проблемы изменено; ");
            req.ProblemDescription = requestDTO.ProblemDescription?.Trim() ?? " нет описания";
        }

        if (req.Priority != requestDTO.Priority)
        {
            commentBuilder.AppendLine($"Приоритет изменен с {req.Priority} на {requestDTO.Priority}; ");
            req.Priority = requestDTO.Priority;
        }

        if (req.EquipmentID != requestDTO.Equipment?.EquipmentID)
        {
            string oldEquipment = req.Equipment.Name;
            string newEquipment = _equipmentRepository.GetByIdAsync(requestDTO.Equipment.EquipmentID).Result?.Name ?? "не назначен";
            commentBuilder.AppendLine($"Оборудование изменено с '{oldEquipment}' на '{newEquipment}'; ");
            req.EquipmentID = requestDTO.Equipment?.EquipmentID;
        }

        if (req.ExecutorID != requestDTO.Executor?.EmployeeID)
        {
            var executor = _employeeRepository.GetByIdAsync(requestDTO.Executor.EmployeeID).Result;

            string oldExecutor = req.Executor.FullName;
            string newExecutor = executor != null ? executor.FullName : "не назначен";
            commentBuilder.AppendLine($"Исполнитель изменен с '{oldExecutor}' на '{newExecutor}';");
            req.ExecutorID = requestDTO.Executor?.EmployeeID;
        }

        if (req.Deadline != requestDTO.Deadline)
        {
            string oldDeadline = req.Deadline.HasValue ? req.Deadline.Value.ToString("dd.MM.yyyy HH:mm") : "не установлен";
            string newDeadline = requestDTO.Deadline.HasValue ? requestDTO.Deadline.Value.ToString("dd.MM.yyyy HH:mm") : "не установлен";
            commentBuilder.AppendLine($"Дедлайн изменен с '{oldDeadline}' на '{newDeadline}'; ");
            req.Deadline = requestDTO.Deadline;
        }

        if (req.CompletionDate != requestDTO.CompletionDate)
        {
            string oldCompletion = req.CompletionDate.HasValue ? req.CompletionDate.Value.ToString("dd.MM.yyyy HH:mm") : "не завершена";
            string newCompletion = requestDTO.CompletionDate.HasValue ? requestDTO.CompletionDate.Value.ToString("dd.MM.yyyy HH:mm") : "не завершена";
            commentBuilder.AppendLine($"Дата завершения изменена с '{oldCompletion}' на '{newCompletion}'; ");
            req.CompletionDate = requestDTO.CompletionDate;
        }

        return commentBuilder.ToString().Trim();
    }
}