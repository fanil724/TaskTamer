import React, { useState, useRef, useEffect } from 'react';
import "./UserProfileDropdown.css";

interface DropDownMenuProps {
    editUser: () => void;
    logout: () => void;
    changePassword: () => void; 
    isLoading?: boolean;
}

const UserProfileDropdown: React.FC<DropDownMenuProps> = ({
                                                              editUser,
                                                              logout,
                                                              changePassword, 
                                                              isLoading = false
                                                          }) => {
    const [isDropdownOpen, setIsDropdownOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setIsDropdownOpen(false);
            }
        };
        const handleEscape = (event: KeyboardEvent) => {
            if (event.key === 'Escape') {
                setIsDropdownOpen(false);
            }
        };

        if (isDropdownOpen) {
            document.addEventListener('mousedown', handleClickOutside);
            document.addEventListener('keydown', handleEscape);
        }

        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
            document.removeEventListener('keydown', handleEscape);
        };
    }, [isDropdownOpen]);

    const toggleDropdown = () => {
        setIsDropdownOpen(!isDropdownOpen);
    };

    const handleEditProfile = () => {
        setIsDropdownOpen(false);
        editUser();
    };

    const handleLogout = () => {
        setIsDropdownOpen(false);
        logout();
    };

    const handleChangePassword = () => {
        setIsDropdownOpen(false);
        changePassword(); 
    };

    return (
        <>
            <div className="user-profile-dropdown" ref={dropdownRef}>
                <button
                    onClick={toggleDropdown}
                    className={`dropdown-toggle ${isDropdownOpen ? 'open' : ''}`}
                    disabled={isLoading}
                    aria-expanded={isDropdownOpen}
                    aria-haspopup="true"
                >
                    Меню пользователя ▼
                    {isLoading && <span className="dropdown-spinner"></span>}
                </button>

                {isDropdownOpen && (
                    <div className="dropdown-menu" role="menu">
                        <div className="dropdown-header">Управление аккаунтом</div>

                        <button
                            onClick={handleEditProfile}
                            className="dropdown-item profile"
                            disabled={isLoading}
                            role="menuitem"
                        >
                            Редактировать профиль
                        </button>

                        <button
                            onClick={handleChangePassword} 
                            className="dropdown-item settings"
                            disabled={isLoading}
                            role="menuitem"
                        >
                            Сменить пароль
                        </button>

                        <div className="dropdown-divider"></div>

                        <button
                            onClick={handleLogout}
                            className="dropdown-item logout"
                            disabled={isLoading}
                            role="menuitem"
                        >
                            {isLoading ? (
                                <span className="dropdown-loading">
                                    <span className="dropdown-spinner"></span>
                                    Выход...
                                </span>
                            ) : (
                                'Выйти'
                            )}
                        </button>
                    </div>
                )}
            </div>
            {isDropdownOpen && <div className="dropdown-backdrop" onClick={() => setIsDropdownOpen(false)} />}
        </>
    );
};

export default UserProfileDropdown;