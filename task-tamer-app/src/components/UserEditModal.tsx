// UserEditModal.tsx
import React, { useState, useEffect } from 'react';
import { IEmployee, IPosition, IDepartment } from "../models/IRequest";
import { IUser, IRole } from "../models/IUser";
import "./UserEditModal.css"

interface UserEditModalProps {
    user: IUser | null;
    isOpen: boolean;
    onClose: () => void;
    onSave: (userData: Partial<IUser>) => void;
    isLoading?: boolean;
}

const UserEditModal: React.FC<UserEditModalProps> = ({
    user,
    isOpen,
    onClose,
    onSave,
    isLoading = false
}) => {
    const [formData, setFormData] = useState<Partial<IUser>>({
        username: '',
        isActive: true,
        roleDTO: {} as IRole,
        employeeDTO: {} as IEmployee
    });

    const [employeeFormData, setEmployeeFormData] = useState<Partial<IEmployee>>({
        fullName: '',
        phone: '',
        email: '',
        userType: '',
        isActive: true,
        positionDTO: {} as IPosition,
        departmentDTO: {} as IDepartment
    });

    const [errors, setErrors] = useState<{ [key: string]: string }>({});
    const [editEmployeeInfo, setEditEmployeeInfo] = useState(false);

    useEffect(() => {
        if (user) {
            setFormData({
                username: user.username,
                isActive: user.isActive,
                roleDTO: user.roleDTO
            });

            if (user.employeeDTO) {
                setEmployeeFormData({
                    fullName: user.employeeDTO.fullName,
                    phone: user.employeeDTO.phone,
                    email: user.employeeDTO.email,
                    userType: user.employeeDTO.userType,
                    isActive: user.employeeDTO.isActive,
                    positionDTO: user.employeeDTO.positionDTO,
                    departmentDTO: user.employeeDTO.departmentDTO
                });
            }
        } 
    }, [user, isOpen]);

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value, type } = e.target;
        const checked = (e.target as HTMLInputElement).checked;

        setFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));

        // Очищаем ошибку при изменении поля
        if (errors[name]) {
            setErrors(prev => ({ ...prev, [name]: '' }));
        }
    };

    const handleEmployeeInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value, type } = e.target;
        const checked = (e.target as HTMLInputElement).checked;

        setEmployeeFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));
    };

    const validateForm = (): boolean => {
        const newErrors: { [key: string]: string } = {};

        if (!formData.username?.trim()) {
            newErrors.username = 'Имя пользователя обязательно';
        }

        if (!formData.roleDTO?.roleID) {
            newErrors.role = 'Роль обязательна';
        }

        if (!user || editEmployeeInfo) {
            if (!employeeFormData.fullName?.trim()) {
                newErrors.fullName = 'ФИО обязательно';
            }

            if (!employeeFormData.email?.trim()) {
                newErrors.email = 'Email обязателен';
            } else if (!/\S+@\S+\.\S+/.test(employeeFormData.email)) {
                newErrors.email = 'Некорректный формат email';
            }

            if (!employeeFormData.positionDTO?.positionID) {
                newErrors.position = 'Должность обязательна';
            }

            if (!employeeFormData.departmentDTO?.departmentID) {
                newErrors.department = 'Отдел обязателен';
            }
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        if (validateForm()) {
            const userData = {
                ...formData,
                employeeDTO: user && !editEmployeeInfo
                    ? user.employeeDTO
                    : { ...employeeFormData } as IEmployee
            };
            onSave(userData);
        }
    };

    const handleClose = () => {
        setErrors({});
        setEditEmployeeInfo(false);
        onClose();
    };

    if (!isOpen) return null;

    return (
        <div className="modal-overlay" onClick={handleClose}>
            <div className="modal-content" onClick={e => e.stopPropagation()}>
                <div className="modal-header">
                    <h2>{user ? 'Редактировать пользователя' : 'Создать пользователя'}</h2>
                    <button className="modal-close" onClick={handleClose}>&times;</button>
                </div>

                <form onSubmit={handleSubmit} className="modal-form">
                    <div className="form-section">
                        <h3>Учетные данные</h3>

                        <div className="form-group">
                            <label htmlFor="username">Имя пользователя *</label>
                            <input
                                type="text"
                                id="username"
                                name="username"
                                value={formData.username || ''}
                                onChange={handleInputChange}
                                className={errors.username ? 'error' : ''}
                                disabled={isLoading}
                            />
                            {errors.username && <span className="error-text">{errors.username}</span>}
                        </div>
                        
                        <div className="form-group">
                            <label>Роль</label>
                            <div className="read-only-field">
                                {formData.roleDTO?.name ? (
                                    `${formData.roleDTO.name} (Уровень доступа: ${formData.roleDTO.accessLevel})`
                                ) : (
                                    'Роль не назначена'
                                )}
                            </div>
                        </div>
                        
                        <div className="form-group checkbox-group">
                            <label>
                                <input
                                    type="checkbox"
                                    name="isActive"
                                    checked={formData.isActive || false}
                                    onChange={handleInputChange}
                                    disabled={isLoading}
                                />
                                Активный пользователь
                            </label>
                        </div>
                    </div>

                    {user && (
                        <div className="form-section">
                            <div className="section-header">
                                <h3>Информация о сотруднике</h3>
                                <button
                                    type="button"
                                    className="btn btn-outline"
                                    onClick={() => setEditEmployeeInfo(!editEmployeeInfo)}
                                    disabled={isLoading}
                                >
                                    {editEmployeeInfo ? 'Отменить редактирование' : 'Редактировать'}
                                </button>
                            </div>
                        </div>
                    )}

                    {(editEmployeeInfo || !user) && (
                        <div className="form-section">
                            <h3>Данные сотрудника</h3>

                            <div className="form-group">
                                <label htmlFor="fullName">ФИО *</label>
                                <input
                                    type="text"
                                    id="fullName"
                                    name="fullName"
                                    value={employeeFormData.fullName || ''}
                                    onChange={handleEmployeeInputChange}
                                    className={errors.fullName ? 'error' : ''}
                                    disabled={isLoading}
                                />
                                {errors.fullName && <span className="error-text">{errors.fullName}</span>}
                            </div>

                            <div className="form-group">
                                <label htmlFor="email">Email *</label>
                                <input
                                    type="email"
                                    id="email"
                                    name="email"
                                    value={employeeFormData.email || ''}
                                    onChange={handleEmployeeInputChange}
                                    className={errors.email ? 'error' : ''}
                                    disabled={isLoading}
                                />
                                {errors.email && <span className="error-text">{errors.email}</span>}
                            </div>

                            <div className="form-group">
                                <label htmlFor="phone">Телефон</label>
                                <input
                                    type="tel"
                                    id="phone"
                                    name="phone"
                                    value={employeeFormData.phone || ''}
                                    onChange={handleEmployeeInputChange}
                                    disabled={isLoading}
                                />
                            </div>
                            
                            <div className="form-group">
                                <label>Должность</label>
                                <div className="read-only-field">
                                    {employeeFormData.positionDTO?.title || 'Должность не указана'}
                                </div>
                            </div>
                            
                            <div className="form-group">
                                <label>Отдел</label>
                                <div className="read-only-field">
                                    {employeeFormData.departmentDTO?.name || 'Отдел не указан'}
                                </div>
                            </div>
                            
                            <div className="form-group">
                                <label htmlFor="userType">Тип пользователя</label>
                                <input
                                    id="userType"
                                    name="userType"
                                    value={employeeFormData.userType || ''} readOnly
                                />
                            </div>

                            <div className="form-group checkbox-group">
                                <label>
                                    <input
                                        type="checkbox"
                                        name="isActive"
                                        checked={employeeFormData.isActive || false}
                                        onChange={handleEmployeeInputChange}
                                        disabled={isLoading}
                                    />
                                    Активный сотрудник
                                </label>
                            </div>
                        </div>
                    )}

                    {user?.employeeDTO && !editEmployeeInfo && (
                        <div className="employee-info">
                            <h4>Текущая информация о сотруднике</h4>
                            <p><strong>ФИО:</strong> {user.employeeDTO.fullName}</p>
                            <p><strong>Должность:</strong> {user.employeeDTO.positionDTO.title}</p>
                            <p><strong>Отдел:</strong> {user.employeeDTO.departmentDTO.name}</p>
                            <p><strong>Email:</strong> {user.employeeDTO.email}</p>
                            <p><strong>Телефон:</strong> {user.employeeDTO.phone}</p>
                            <p><strong>Тип:</strong> {user.employeeDTO.userType}</p>
                            <p><strong>Статус:</strong> {user.employeeDTO.isActive ? 'Активен' : 'Неактивен'}</p>
                            <p><strong>Дата регистрации:</strong> {new Date(user.employeeDTO.registrationDate).toLocaleDateString()}</p>
                            {user.employeeDTO.terminationDate && (
                                <p><strong>Дата увольнения:</strong> {new Date(user.employeeDTO.terminationDate).toLocaleDateString()}</p>
                            )}
                        </div>
                    )}

                    <div className="modal-actions">
                        <button
                            type="button"
                            onClick={handleClose}
                            className="btn btn-secondary"
                            disabled={isLoading}
                        >
                            Отмена
                        </button>
                        <button
                            type="submit"
                            className="btn btn-primary"
                            disabled={isLoading}
                        >
                            {isLoading ? 'Сохранение...' : 'Сохранить'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default UserEditModal;