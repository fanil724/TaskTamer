using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;

namespace TaskTamer_Application.Service
{
    public class RoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public RoleService(IRoleRepository roleRepository, IUserRepository userRepository)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
        }

        public async Task<OperationResult<int>> CreateRoleAsync(RoleDTO roleDto)
        {
            try
            {
                _logger.Info($"Создание роли: {roleDto.Name}");
                if (roleDto == null)
                {
                    _logger.Warn("Попытка создания пустой роли");
                    return OperationResult<int>.Failure("Данные роли не предоставлены");
                }
               
                var validationResult = ValidateRoleDto(roleDto);
                if (!validationResult.IsSuccess)
                {
                    _logger.Warn($"Ошибка валидации: {validationResult.Message}");
                    return OperationResult<int>.Failure(validationResult.Message);
                }
               
                var existingRole = await _roleRepository.GetByName(roleDto.Name);
                if (existingRole != null)
                {
                    _logger.Warn($"Роль с именем '{roleDto.Name}' уже существует");
                    return OperationResult<int>.Failure("Роль с таким именем уже существует");
                }
                var role = new Role
                {
                    Name = roleDto.Name.Trim(),
                    Description = roleDto.Description?.Trim(),
                    AccessLevel = roleDto.AccessLevel
                };

                var roleId = await _roleRepository.Create(role);

                _logger.Info($"Роль '{roleDto.Name}' создана с ID: {roleId}");
                return OperationResult<int>.Success(roleId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при создании роли '{roleDto?.Name}'");
                return OperationResult<int>.Failure("Ошибка при создании роли");
            }
        }

        public async Task<OperationResult<RoleDTO>> GetRoleByNameAsync(string name)
        {
            try
            {
                _logger.Debug($"Запрос роли по имени: {name}");
                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.Warn("Неверное имя роли для запроса");
                    return OperationResult<RoleDTO>.Failure("Имя роли не указано");
                }
                var role = await _roleRepository.GetByName(name.Trim());
                if (role == null)
                {
                    _logger.Warn($"Роль с именем '{name}' не найдена");
                    return OperationResult<RoleDTO>.Failure("Роль не найдена");
                }
                var roleDto = new RoleDTO(role);
                _logger.Debug($"Роль с именем '{name}' успешно получена");
                return OperationResult<RoleDTO>.Success(roleDto);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при получении роли с именем '{name}'");
                return OperationResult<RoleDTO>.Failure("Ошибка при получении роли");
            }
        }

        public async Task<OperationResult<IEnumerable<RoleDTO>>> GetAllRolesAsync()
        {
            try
            {
                _logger.Debug("Запрос всех ролей");

                var roles = await _roleRepository.GetAllAsync();

                if (roles == null || !roles.Any())
                {
                    _logger.Info("Роли не найдены");
                    return OperationResult<IEnumerable<RoleDTO>>.Success(Enumerable.Empty<RoleDTO>());
                }

                var roleDtos = roles.Select(r => new RoleDTO(r)).ToList();
                _logger.Debug($"Получено {roleDtos.Count} ролей");
                return OperationResult<IEnumerable<RoleDTO>>.Success(roleDtos);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при получении списка ролей");
                return OperationResult<IEnumerable<RoleDTO>>.Failure("Ошибка при получении списка ролей");
            }
        }

        public async Task<OperationResult> UpdateRoleAsync(RoleDTO roleDto)
        {
            try
            {
                _logger.Info($"Обновление роли с ID: {roleDto.RoleID}");
                if (roleDto == null)
                {
                    _logger.Warn("Попытка обновления пустой роли");
                    return OperationResult.Failure("Данные роли не предоставлены");
                }
                
                var validationResult = ValidateRoleDto(roleDto);
                if (!validationResult.IsSuccess)
                {
                    _logger.Warn($"Ошибка валидации: {validationResult.Message}");
                    return OperationResult.Failure(validationResult.Message);
                }
                
                var existingRole = await _roleRepository.GetById(roleDto.RoleID);
                if (existingRole == null)
                {
                    _logger.Warn($"Роль с ID {roleDto.RoleID} не найдена");
                    return OperationResult.Failure("Роль не найдена");
                }
                
                if (!string.Equals(existingRole.Name, roleDto.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var roleWithSameName = await _roleRepository.GetByName(roleDto.Name);
                    if (roleWithSameName != null)
                    {
                        _logger.Warn($"Роль с именем '{roleDto.Name}' уже существует");
                        return OperationResult.Failure("Роль с таким именем уже существует");
                    }
                }
                                
                existingRole.Name = roleDto.Name.Trim();
                existingRole.Description = roleDto.Description?.Trim();
                existingRole.AccessLevel = roleDto.AccessLevel;

                var result = await _roleRepository.Update(existingRole);
                if (result > 0)
                {
                    _logger.Info($"Роль с ID {roleDto.RoleID} успешно обновлена");
                    return OperationResult.Success();
                }
                else
                {
                    _logger.Warn($"Не удалось обновить роль с ID {roleDto.RoleID}");
                    return OperationResult.Failure("Не удалось обновить роль");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при обновлении роли с ID {roleDto?.RoleID}");
                return OperationResult.Failure("Ошибка при обновлении роли");
            }
        }
        public async Task<OperationResult> DeleteRoleAsync(int id)
        {
            try
            {
                _logger.Info($"Удаление роли с ID: {id}");
                if (id <= 0)
                {
                    _logger.Warn("Неверный ID роли для удаления");
                    return OperationResult.Failure("Неверный идентификатор роли");
                }

                var existingRole = await _roleRepository.GetById(id);
                if (existingRole == null)
                {
                    _logger.Warn($"Роль с ID {id} не найдена для удаления");
                    return OperationResult.Failure("Роль не найдена");
                }
                 var usersWithRole = await _userRepository.GetUserWithRolesAllAsync(id);
                if (usersWithRole.Any())
                {
                    _logger.Warn($"Невозможно удалить роль с ID {id} - она используется пользователями");
                    return OperationResult.Failure("Невозможно удалить роль - она используется пользователями");
                }

                var result = await _roleRepository.Delete(id);
                if (result > 0)
                {
                    _logger.Info($"Роль с ID {id} успешно удалена");
                    return OperationResult.Success();
                }
                else
                {
                    _logger.Warn($"Не удалось удалить роль с ID {id}");
                    return OperationResult.Failure("Не удалось удалить роль");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при удалении роли с ID {id}");
                return OperationResult.Failure("Ошибка при удалении роли");
            }
        }
        public async Task<OperationResult<RoleDTO>> GetRoleByIdAsync(int id)
        {
            try
            {
                _logger.Debug($"Запрос роли с ID: {id}");
                if (id <= 0)
                {
                    _logger.Warn("Неверный ID роли для запроса");
                    return OperationResult<RoleDTO>.Failure("Неверный идентификатор роли");
                }
                var role = await _roleRepository.GetById(id);
                if (role == null)
                {
                    _logger.Warn($"Роль с ID {id} не найдена");
                    return OperationResult<RoleDTO>.Failure("Роль не найдена");
                }
                var roleDto = new RoleDTO(role);
                _logger.Debug($"Роль с ID {id} успешно получена");
                return OperationResult<RoleDTO>.Success(roleDto);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при получении роли с ID {id}");
                return OperationResult<RoleDTO>.Failure("Ошибка при получении роли");
            }
        }
        private OperationResult ValidateRoleDto(RoleDTO roleDto)
        {
            if (string.IsNullOrWhiteSpace(roleDto.Name))
                return OperationResult.Failure("Название роли обязательно");

            if (roleDto.Name.Length > 50)
                return OperationResult.Failure("Название роли слишком длинное");

            if (!string.IsNullOrWhiteSpace(roleDto.Description) && roleDto.Description.Length > 200)
                return OperationResult.Failure("Описание роли слишком длинное");

            if (roleDto.AccessLevel < 1 || roleDto.AccessLevel > 10)
                return OperationResult.Failure("Уровень доступа должен быть от 1 до 10");

            return OperationResult.Success();
        }
    }
}
