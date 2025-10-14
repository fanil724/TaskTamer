import React, {FC, useContext, useState} from 'react';
import {Context} from "../index";
import './LoginForm.css';
import {observer} from "mobx-react-lite";

const LoginForm: FC = () => {
    const [login, setLogin] = useState<string>('');
    const [password, setPassword] = useState<string>('');
    const [error, setError] = useState<string>('');
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const {store} = useContext(Context);

    const handleLogin = async () => {
        if (!login.trim() || !password.trim()) {
            setError('Логин и пароль обязательны для заполнения');
            return;
        }

        setError('');
        setIsLoading(true);

        try {
            await store.login(login, password);
            // Если авторизация успешна, ошибка автоматически очистится
        } catch (error: any) {
            console.error('Login error:', error);
            setError(error.response?.data?.message || 'Ошибка авторизации. Проверьте логин и пароль');
        } finally {
            setIsLoading(false);
        }
    };

    const handleKeyPress = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter') {
            handleLogin();
        }
    };

    const clearError = () => {
        setError('');
    };

    return (
        <div className="login-container">
            <div className="login-form">
                <h2 className="login-title">Вход в систему</h2>

                {error && (
                    <div className="error-message">
                        <span>{error}</span>
                        <button
                            onClick={clearError}
                            className="error-close"
                            aria-label="Закрыть ошибку"
                        >
                            ×
                        </button>
                    </div>
                )}

                <div className="input-group">
                    <input
                        type="text"
                        value={login}
                        placeholder=" "
                        onChange={e => {
                            setLogin(e.target.value);
                            clearError();
                        }}
                        onKeyPress={handleKeyPress}
                        className={`login-input ${error ? 'input-error' : ''}`}
                        disabled={isLoading}
                    />
                    <label className="input-label">Логин</label>
                </div>

                <div className="input-group">
                    <input
                        type="password"
                        value={password}
                        placeholder=" "
                        onChange={e => {
                            setPassword(e.target.value);
                            clearError();
                        }}
                        onKeyPress={handleKeyPress}
                        className={`login-input ${error ? 'input-error' : ''}`}
                        disabled={isLoading}
                    />
                    <label className="input-label">Пароль</label>
                </div>

                <button
                    onClick={handleLogin}
                    className={`login-button ${isLoading ? 'loading' : ''}`}
                    disabled={isLoading}
                >
                    {isLoading ? (
                        <>
                            <span className="button-spinner"></span>
                            Вход...
                        </>
                    ) : (
                        'Войти'
                    )}
                </button>
            </div>
        </div>
    );
};

export default observer(LoginForm);