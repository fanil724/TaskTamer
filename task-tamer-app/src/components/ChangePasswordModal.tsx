import React, { useState } from 'react';
import PasswordService from '../services/PasswordService';
import { useContext } from 'react';
import { Context } from '../index';
import './ChangePasswordModal.css';

interface ChangePasswordModalProps {
    isOpen: boolean;
    onClose: () => void;
}

const ChangePasswordModal: React.FC<ChangePasswordModalProps> = ({
    isOpen,
    onClose
}) => {
    const { store } = useContext(Context);
    const [OLDPassword, setCurrentPassword] = useState('');
    const [NewPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    const validateForm = (): boolean => {
        setError('');

        if (!OLDPassword || !NewPassword || !confirmPassword) {
            setError('Все поля обязательны для заполнения');
            return false;
        }

        if (NewPassword !== confirmPassword) {
            setError('Новый пароль и подтверждение не совпадают');
            return false;
        }

        if (NewPassword.length < 8) {
            setError('Пароль должен содержать минимум 8 символов');
            return false;
        }

        if (!/(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?":{}|<>])/.test(NewPassword)) {
            setError('Пароль должен содержать заглавные и строчные буквы, цифры и специальные символы');
            return false;
        }

        if (OLDPassword === NewPassword) {
            setError('Новый пароль не должен совпадать с текущим');
            return false;
        }

        return true;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validateForm()) return;

        setIsLoading(true);
        setError('');
        setSuccess('');

        try {
            const changePasswordData = {
                OLDPassword,
                NewPassword               
            };

            const response = await PasswordService.changePassword(changePasswordData);

            if (response.status === 200) {
                setSuccess('Пароль успешно изменен');
                setTimeout(() => {
                    onClose();                    
                    setCurrentPassword('');
                    setNewPassword('');
                    setConfirmPassword('');
                    setSuccess('');
                }, 2000);
            }
        } catch (error: any) {
            console.error('Password change error:', error);

            if (error.response?.status === 401) {
                setError('Неверный текущий пароль');
            } else if (error.response?.status === 400) {
                setError(error.response.data.message || 'Некорректные данные');
            } else if (error.response?.status === 500) {
                setError('Ошибка сервера. Попробуйте позже');
            } else {
                setError('Произошла ошибка при смене пароля');
            }
        } finally {
            setIsLoading(false);
        }
    };

    const handleClose = () => {
        setCurrentPassword('');
        setNewPassword('');
        setConfirmPassword('');
        setError('');
        setSuccess('');
        onClose();
    };

    if (!isOpen) return null;

    return (
        <div className="cp-modal-overlay">
            <div className="cp-modal-content">
                <div className="cp-modal-header">
                    <h2 className="cp-modal-title">Смена пароля</h2>
                    <button
                        className="cp-modal-close"
                        onClick={handleClose}
                        disabled={isLoading}
                        aria-label="Закрыть окно"
                    >
                        ×
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="cp-password-form">
                    {error && (
                        <div className="cp-error-message">
                            <span className="cp-error-text">{error}</span>
                            <button
                                onClick={() => setError('')}
                                className="cp-error-close"
                                type="button"
                                aria-label="Закрыть ошибку"
                            >
                                ×
                            </button>
                        </div>
                    )}

                    {success && (
                        <div className="cp-success-message">
                            <span className="cp-success-text">{success}</span>
                        </div>
                    )}

                    <div className="cp-form-group">
                        <label htmlFor="cp-current-password" className="cp-form-label">
                            Текущий пароль
                        </label>
                        <input
                            id="cp-current-password"
                            type="password"
                            value={OLDPassword}
                            onChange={(e) => setCurrentPassword(e.target.value)}
                            disabled={isLoading}
                            required
                            className="cp-form-input"
                        />
                    </div>

                    <div className="cp-form-group">
                        <label htmlFor="cp-new-password" className="cp-form-label">
                            Новый пароль
                        </label>
                        <input
                            id="cp-new-password"
                            type="password"
                            value={NewPassword}
                            onChange={(e) => setNewPassword(e.target.value)}
                            disabled={isLoading}
                            required
                            placeholder="Минимум 8 символов, включая A-Z, a-z, 0-9, !@#$%"
                            className="cp-form-input"
                        />
                    </div>

                    <div className="cp-form-group">
                        <label htmlFor="cp-confirm-password" className="cp-form-label">
                            Подтвердите новый пароль
                        </label>
                        <input
                            id="cp-confirm-password"
                            type="password"
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                            disabled={isLoading}
                            required
                            className="cp-form-input"
                        />
                    </div>

                    <div className="cp-password-requirements">
                        <p className="cp-requirements-title">Требования к паролю:</p>
                        <ul className="cp-requirements-list">
                            <li className={NewPassword.length >= 8 ? 'cp-requirement-valid' : 'cp-requirement'}>
                                ≥ 8 символов
                            </li>
                            <li className={/[A-Z]/.test(NewPassword) ? 'cp-requirement-valid' : 'cp-requirement'}>
                                Заглавные буквы (A-Z)
                            </li>
                            <li className={/[a-z]/.test(NewPassword) ? 'cp-requirement-valid' : 'cp-requirement'}>
                                Строчные буквы (a-z)
                            </li>
                            <li className={/\d/.test(NewPassword) ? 'cp-requirement-valid' : 'cp-requirement'}>
                                Цифры (0-9)
                            </li>
                            <li className={/[!@#$%^&*(),.?":{}|<>]/.test(NewPassword) ? 'cp-requirement-valid' : 'cp-requirement'}>
                                Специальные символы
                            </li>
                        </ul>
                    </div>

                    <div className="cp-modal-actions">
                        <button
                            type="button"
                            className="cp-btn cp-btn-secondary"
                            onClick={handleClose}
                            disabled={isLoading}
                        >
                            Отмена
                        </button>
                        <button
                            type="submit"
                            className="cp-btn cp-btn-primary"
                            disabled={isLoading}
                        >
                            {isLoading ? 'Смена пароля...' : 'Сменить пароль'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default ChangePasswordModal;