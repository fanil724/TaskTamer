using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;

namespace TaskTamer_Application.Service
{
    public class RequestHistoryService
    {
        private readonly IRequestHistoryRepository _requestHistoryRepository;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();


        public RequestHistoryService(IRequestHistoryRepository requestHistoryRepository)
        {
            _requestHistoryRepository = requestHistoryRepository;
        }
        public async Task<OperationResult<List<RequestHistoryDTO>>> GetRequestHistoryByIdAsync(int id)
        {
            try
            {
                _logger.Debug($"Запрос история изменений заявки с ID: {id}");

                if (id <= 0)
                {
                    _logger.Warn("Неверный ID заявки для запроса история изменений");
                    return OperationResult<List<RequestHistoryDTO>>.Failure("Неверный идентификатор заявки для запроса история изменений");
                }

                var request = await _requestHistoryRepository.GetByIdAsync(id);

                if (request.Count() == 0)
                {
                    _logger.Warn($"История изменений заявки с ID {id} не найдена");
                    return OperationResult<List<RequestHistoryDTO>>.Failure($"История изменений заявки с ID {id} не найдена");
                }

                var requestHistDto = request.Select(x => new RequestHistoryDTO(x??new RequestHistory())).ToList();
                _logger.Debug($"Заявка с ID {id} успешно получена");
                return OperationResult<List<RequestHistoryDTO>>.Success(requestHistDto ?? new List<RequestHistoryDTO>());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при получении заявки с ID {id}");
                return OperationResult<List<RequestHistoryDTO>>.Failure("Ошибка при получении заявки");
            }
        }

    }
}
