using Microsoft.AspNetCore.Identity;
using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;

namespace TaskTamer_Application.Service;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly EmployeeService _employeeService;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public UserService(IUserRepository userRepository, IEmployeeRepository employeeRepository,
        IRoleRepository roleStores, EmployeeService employeeService)
    {
        _employeeRepository = employeeRepository;
        _roleRepository = roleStores;
        _userRepository = userRepository;
        _employeeService = employeeService;
    }

    public async Task<OperationResult<UserDTO>> GetUserIdAsync(int userId)
    {
        if (userId <= 0)
            return OperationResult<UserDTO>.Failure("Неверный идентификатор пользователя");

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return OperationResult<UserDTO>.Failure("Пользователь не найден");

            if (!user.IsActive)
                return OperationResult<UserDTO>.Failure("Пользователь деактивирован");

            return OperationResult<UserDTO>.Success(new UserDTO(user));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при получении пользователя с ID {userId}");
            return OperationResult<UserDTO>.Failure("Ошибка при получении данных пользователя");
        }
    }

    public async Task<OperationResult<UserDTO>> GetUserRoleAsync(string role)
    {
        if (string.IsNullOrEmpty(role))
            return OperationResult<UserDTO>.Failure("Некорректное имя пользователя");

        try
        {
            var user = await _userRepository.GetUserWithRolesAsync(role);

            if (user == null)
                return OperationResult<UserDTO>.Failure("Пользователь не найден");

            if (!user.IsActive)
                return OperationResult<UserDTO>.Failure("Пользователь деактивирован");

            return OperationResult<UserDTO>.Success(new UserDTO(user));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при получении пользователя  {role}");
            return OperationResult<UserDTO>.Failure("Ошибка при получении данных пользователя");
        }
    }

    public async Task<OperationResult<IEnumerable<UserDTO>>> GetAllAsync(bool onlyActive = true)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();

            if (onlyActive)
                users = users.Where(u => u.IsActive);

            if (!users.Any())
                return OperationResult<IEnumerable<UserDTO>>.Failure("Пользователи не найдены");

            var result = users.Select(u => new UserDTO(u)).ToList();
            return OperationResult<IEnumerable<UserDTO>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при получении списка пользователей");
            return OperationResult<IEnumerable<UserDTO>>.Failure("Ошибка при получении списка пользователей");
        }
    }

    public async Task<OperationResult> Create(UserDTO userDto)
    {
        if (userDto == null)
            return OperationResult.Failure("Данные пользователя не предоставлены");

        
        var existingUser = await _userRepository.GetByUsernameAsync(userDto.Username);
        if (existingUser != null)
            return OperationResult.Failure("Пользователь с таким логином уже существует");

        
        var employeeExists = await _employeeRepository.GetByNameAsync(userDto.employeeDTO.FullName);
        if (employeeExists != null)
            return OperationResult.Failure("Указанный сотрудник уже существует");


        var emp = _employeeService.CreateEmployeeAsync(userDto.employeeDTO).Result;
        if (!emp.IsSuccess)
        {
            return OperationResult.Failure($"Не удалось создать пользователя {emp.Message}");
        }

        userDto.employeeDTO.EmployeeID = emp.Data;

        var roleExists = await _roleRepository.GetById(userDto.roleDTO.RoleID);
        if (roleExists == null)
            return OperationResult.Failure("Указанная роль не найдена");

        var passwordValidation = ValidatePassword(userDto.PasswordHash);
        if (!passwordValidation.IsSuccess)
        {
            _logger.Warn($"Ошибка валидации пароля: {passwordValidation.Message}");
            return OperationResult<int>.Failure(passwordValidation.Message);
        }

        
        var passwordHasher = new PasswordHasher<User>();
        var user = new User
        {
            Username = userDto.Username,
            PasswordHash = passwordHasher.HashPassword(null, userDto.PasswordHash),
            EmployeeID = emp.Data,
            RoleID = roleExists.RoleID,
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        
        await _userRepository.AddAsync(user);

        return OperationResult.Success("Пользователь успешно создан");
    }

    public async Task<OperationResult> Update(UserDTO userDto)
    {
        if (userDto == null)
        {
            return OperationResult.Failure("Данные пользователя не предоставлены");
        }
        if (userDto.UserID <= 0)
        {
            return OperationResult.Failure("Неверный идентификатор пользователя");
        }

        var existingUser = await _userRepository.GetByIdAsync(userDto.UserID);
        if (existingUser == null)
            return OperationResult.Failure("Пользователь не найден");

        if (!string.Equals(existingUser.Username, userDto.Username, StringComparison.OrdinalIgnoreCase))
        {
            var userWithSameUsername = await _userRepository.GetByUsernameAsync(userDto.Username);
            if (userWithSameUsername != null)
                return OperationResult.Failure("Пользователь с таким логином уже существует");
        }


        if (userDto.employeeDTO == null || userDto.employeeDTO.EmployeeID <= 0)
            return OperationResult.Failure("Неверно указан сотрудник");

        if (userDto.roleDTO == null || userDto.roleDTO.RoleID <= 0)
            return OperationResult.Failure("Неверно указана роль");

        var employeeExists = await _employeeRepository.GetByIdAsync(userDto.employeeDTO.EmployeeID);
        if (employeeExists == null)
            return OperationResult.Failure("Указанный сотрудник не найден");



        var roleExists = await _roleRepository.GetById(userDto.roleDTO.RoleID);
        if (roleExists == null)
            return OperationResult.Failure("Указанная роль не найдена");


        existingUser.Username = userDto.Username.Trim();
        existingUser.EmployeeID = employeeExists.EmployeeID;
        existingUser.RoleID = roleExists.RoleID;
        existingUser.IsActive = userDto.IsActive;


        if (!string.IsNullOrWhiteSpace(userDto.PasswordHash))
        {
            var passwordHasher = new PasswordHasher<User>();
            existingUser.PasswordHash = passwordHasher.HashPassword(null, userDto.PasswordHash);
        }

        try
        {
            await _employeeService.UpdateEmployeeAsync(userDto.employeeDTO);
            await _userRepository.UpdateAsync(existingUser);


            return OperationResult.Success("Пользователь успешно обновлен");
        }
        catch (Exception ex)
        {
            return OperationResult.Failure("Произошла ошибка при обновлении пользователя");
        }
    }

    public async Task<OperationResult> Delete(int id)
    {
        if (id <= 0)
            return OperationResult<UserDTO>.Failure("Неверный идентификатор пользователя");


        try
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                return OperationResult<UserDTO>.Failure("Пользователь не найден");

            await _userRepository.DeleteAsync(id);


            return OperationResult.Success("Пользователь успешно удален");
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Произошла ошибка при удаление пользователя {ex.Message} ");
        }
    }

    public async Task<OperationResult<UserDTO>> ChangeStatus(int id)
    {
        if (id <= 0)
        {
            return OperationResult<UserDTO>.Failure("Неверный идентификатор пользователя");
        }

        var us = await _userRepository.GetByIdAsync(id);

        try
        {
            us.IsActive = !us.IsActive;
            if (!us.IsActive)
            {
                us.Employee.TerminationDate = DateTime.Now;
            }


            await _userRepository.UpdateAsync(us);


            return OperationResult<UserDTO>.Success(new UserDTO(us), "Статус упешно обновлен");
        }
        catch (Exception ex)
        {
            return OperationResult<UserDTO>.Failure(
                $"Произошла ошибка при имзменение статуса пользователя {ex.Message}");
        }
    }

    private OperationResult ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return OperationResult.Failure("Пароль не может быть пустым");
        }
        if (password.Length < 6)
        {
            return OperationResult.Failure("Пароль должен содержать 6 символов");
        }
        if (password.Length > 100)
        {
            return OperationResult.Failure("Пароль не может превышать 100 символов");
        }

        if (!password.Any(char.IsUpper))
        {
            return OperationResult.Failure("Пароль должен содержать одну заглавную букву");
        }
        if (!password.Any(char.IsLower))
        {
            return OperationResult.Failure("Пароль должен содержать одну строчную букву");
        }
        if (!password.Any(char.IsDigit))
        {
            return OperationResult.Failure("Пароль должен содержать одну цифру");
        }
        return OperationResult.Success();
    }
}