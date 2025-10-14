import React, { useContext, useEffect, useState, useCallback } from 'react';
import LoginForm from "./components/LoginForm";
import { Context } from "./index";
import { observer } from "mobx-react-lite";
import "./App.css";
import RequestService from "./services/RequestService";
import { IRequest, IRequestType, IEquipment, IEmployee } from "./models/IRequest";
import RequestRow from "./components/RequestRow";
import CreateRequestModal from "./components/CreateRequestModal";
import UserEditModal from './components/UserEditModal';
import { IUser } from './models/IUser';
import UserService from './services/UserService';
import UserProfileDropdown from './components/UserProfileDropdown';
import ChangePasswordModal from "./components/ChangePasswordModal";
import EditRequestModal from "./components/EditRequestModal";

function App() {
    const { store } = useContext(Context);
    const [isLoading, setIsLoading] = useState(true);
    const [requests, setRequests] = useState<IRequest[]>([]);
    const [requestsLoading, setRequestsLoading] = useState(false);
    const [userLoading, setUserLoading] = useState(false);
    const [logoutLoading, setLogoutLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [isEditUserModalOpen, setIsEditUserModalOpen] = useState(false);

    const [requestTypes, setRequestTypes] = useState<IRequestType[]>([]);
    const [equipmentList, setEquipmentList] = useState<IEquipment[]>([]);
    const [employees, setEmployees] = useState<IEmployee[]>([]);
    const [currentUser, setCurrentUser] = useState<IUser | null>(null);
    const [isChangePasswordModalOpen, setIsChangePasswordModalOpen] = useState(false);

    const [isEditModalOpen, setIsEditModalOpen] = useState(false);
    const [selectedRequest, setSelectedRequest] = useState<IRequest | null>(null);
    const [updateLoading, setUpdateLoading] = useState(false);

    const [sseConnection, setSseConnection] = useState<EventSource | null>(null);
    const [highlightedRequests, setHighlightedRequests] = useState<Set<number>>(new Set());
    const [notifications, setNotifications] = useState<{ id: number, message: string }[]>([]);



    const checkAuthAndExecute = async (action: () => Promise<void>) => {
        try {
            const isAuthenticated = await store.checkAuth();
            if (!isAuthenticated) {
                setError('Сессия истекла. Пожалуйста, войдите снова.');
                return;
            }
            await action();
        } catch (error) {
            console.error('Auth check failed:', error);
            setError('Ошибка проверки авторизации');
        }
    };

    useEffect(() => {
        const initializeAuth = async () => {
            try {
                await store.checkAuth();
            } catch (error) {
                console.error('Auth check failed:', error);
            } finally {
                setIsLoading(false);
            }
        };
        initializeAuth();
    }, []);

    useEffect(() => {
        if (store.isAuth) {
            checkAuthAndExecute(async () => {
                await fetchRequests();
                await fetchUserData();
                await loadFormData();
            });
        }
    }, [store.isAuth]);
    useEffect(() => {
        if (store.isAuth) {
            connectToSSE();
        } else {
            if (sseConnection) {
                sseConnection.close();
                setSseConnection(null);
            }
        }

        return () => {
            if (sseConnection) {
                sseConnection.close();
            }
        };
    }, [store.isAuth]);

    const handleRequestClick = (request: IRequest) => {
        checkAuthAndExecute(async () => {
            removeHighlight(request.requestID);

            setSelectedRequest(request);
            setIsEditModalOpen(true);
        });
    };

    const handleLogout = async () => {
        await checkAuthAndExecute(async () => {
            setLogoutLoading(true);
            try {
                await store.logout();
            } catch (error) {
                console.error('Logout error:', error);
                setError('Ошибка при выходе из системы');
            } finally {
                setLogoutLoading(false);
            }
        });
    };

    const fetchFormData = async () => {
        try {
            const [typesResponse, equipmentResponse, employeesResponse] = await Promise.all([
                RequestService.fetchRequestTypes(),
                RequestService.fetchEquipment(),
                RequestService.fetchEmployees()
            ]);

            setRequestTypes(typesResponse.data);
            setEquipmentList(equipmentResponse.data);
            setEmployees(employeesResponse.data);
        } catch (error) {
            console.error('Ошибка загрузки данных для формы:', error);
        }
    };

    const loadFormData = async () => {
        try {
            const [typesResponse, equipmentResponse, employeesResponse] = await Promise.all([
                RequestService.fetchRequestTypes(),
                RequestService.fetchEquipment(),
                RequestService.fetchEmployees()
            ]);

            setRequestTypes(typesResponse.data);
            setEquipmentList(equipmentResponse.data);
            setEmployees(employeesResponse.data);
        } catch (error) {
            console.error('Ошибка загрузки данных для формы:', error);
        }
    };

    const fetchUserData = async () => {
        try {
            const response = await UserService.getUserById(store.userid);
            setCurrentUser(response.data);
        } catch (error) {
            console.error('Ошибка загрузки данных пользователя:', error);
        }
    };

    const handleOpenCreateModal = async () => {
        await checkAuthAndExecute(async () => {
            setIsCreateModalOpen(true);
            await fetchFormData();
        });
    };

    const handleOpenEditUserModal = async () => {
        await checkAuthAndExecute(async () => {
            setIsEditUserModalOpen(true);
        });
    };

    const handleUpdateRequest = async (requestData: IRequest) => {
        await checkAuthAndExecute(async () => {
            setUpdateLoading(true);
            setError(null);

            try {
                const res = await RequestService.updateRequest(requestData);
                await handleRefreshRequests();
                setIsEditModalOpen(false);
                setError(null);
            } catch (error: any) {
                console.error('Ошибка обновления заявки:', error);
                setError(error.response?.data?.message || 'Ошибка обновления заявки');
            } finally {
                setUpdateLoading(false);
            }
        });
    };

    const fetchRequests = async () => {
        setRequestsLoading(true);
        setError(null);
        try {
            const response = await RequestService.fetchRequest();
            setRequests(response.data);
        } catch (error: any) {
            console.error('Failed to fetch requests:', error);
            setError(error.response?.data?.message || 'Ошибка загрузки заявок');
        } finally {
            setRequestsLoading(false);
        }
    };

    const handleCreateRequest = async (requestData: IRequest) => {
        await checkAuthAndExecute(async () => {
            try {
                setRequestsLoading(true);

                const author: IEmployee = {
                    employeeID: store.employeeId,
                    fullName: store.username || 'Неизвестный пользователь',
                    positionDTO: {
                        positionID: 0,
                        title: 'Не указано',
                        description: '',
                        accessLevel: 0
                    },
                    departmentDTO: {
                        departmentID: 0,
                        name: store.department || 'Не указано',
                        description: '',
                        departmentType: '',
                        creationDate: new Date().toISOString(),
                        isActive: true
                    },
                    phone: '',
                    email: '',
                    userType: 'Employee',
                    registrationDate: new Date().toISOString(),
                    terminationDate: null,
                    isActive: true
                };

                const submitData: IRequest = {
                    ...requestData,
                    requestID: 0,
                    creationDate: new Date().toISOString(),
                    author: author,
                    requestStatus: {
                        statusID: 1,
                        name: "Новая",
                        description: "Новая заявка",
                        processingOrder: 1
                    },
                    completionDate: new Date().toISOString(),
                    history: []
                };

                await RequestService.createRequest(submitData);

                await handleRefreshRequests();
                setIsCreateModalOpen(false);
                setError(null);

            } catch (error: any) {
                console.error('Ошибка создания заявки:', error);
                setError(error.response?.data?.message || 'Ошибка создания заявки');
            } finally {
                setRequestsLoading(false);
            }
        });
    };

    const handleEditUser = async (userData: Partial<IUser>) => {
        await checkAuthAndExecute(async () => {
            try {
                setUserLoading(true);
                if (!currentUser) {
                    throw new Error('Пользователь не найден');
                }

                const submitData: IUser = {
                    ...currentUser,
                    ...userData,
                    userID: currentUser.userID,
                    employeeDTO: {
                        ...currentUser.employeeDTO,
                        ...(userData.employeeDTO && {
                            fullName: userData.employeeDTO.fullName,
                            phone: userData.employeeDTO.phone,
                            email: userData.employeeDTO.email
                        })
                    },
                    roleDTO: currentUser.roleDTO
                };

                await UserService.updateUser(submitData);

                setIsEditUserModalOpen(false);
                setError(null);
                await fetchUserData();

            } catch (error: any) {
                console.error('Ошибка обновления пользователя:', error);
                setError(error.response?.data?.message || 'Ошибка обновления пользователя');
            } finally {
                setUserLoading(false);
            }
        });
    };

    const handleSSENotification = useCallback((event: MessageEvent) => {
        console.log(event.data);
        try {
            const notification = JSON.parse(event.data);
            console.log(notification);
            console.log(event.data);
            if (notification.EventType === "notification") {
                const requestId = notification.RequestId;

                setHighlightedRequests(prev => new Set(prev).add(requestId));

                const eventAction = notification.EventName === "create" ? "создана" :
                    notification.EventName === "update" ? "обновлена" : "изменена";

                setNotifications(prev => [
                    ...prev,
                    {
                        id: Date.now(),
                        message: `Заявка #${requestId} ${eventAction} пользователем ${notification.userName}`
                    }
                ]);
                setTimeout(() => {
                    setNotifications(prev => prev.filter(n => n.id !== Date.now()));
                }, 5000);

                handleRefreshRequests();
            }
        } catch (error) {
            console.error('Ошибка обработки SSE уведомления:', error);
        }
    }, []);


    const handleChangePassword = () => {
        checkAuthAndExecute(async () => {
            setIsChangePasswordModalOpen(true);
        });
    };

    const handleRefreshRequests = async () => {
        await checkAuthAndExecute(async () => {
            await fetchRequests();
        });
    };

    const createRequest = () => {
        handleOpenCreateModal();
    };

    const removeHighlight = useCallback((requestId: number) => {
        setHighlightedRequests(prev => {
            const newSet = new Set(prev);
            newSet.delete(requestId);
            return newSet;
        });
    }, []);

    const connectToSSE = useCallback(() => {
        if (sseConnection) {
            sseConnection.close();
        }

        const eventSource = new EventSource('http://localhost:5171/notifications-stream', {
            withCredentials: true
        });

        eventSource.addEventListener('eventmessage', handleSSENotification)


        eventSource.onerror = (error) => {
            console.error('SSE connection error:', error);
            setTimeout(connectToSSE, 5000);
        };

        setSseConnection(eventSource);
    }, [handleSSENotification]);

    const editUser = () => {
        handleOpenEditUserModal();
    };

    if (isLoading) {
        return (
            <div className="loading-container">
                <div className="loading-spinner"></div>
                <p>Проверка авторизации...</p>
            </div>
        );
    }

    if (!store.isAuth) {
        return <LoginForm />;
    }

    const userRequests = requests.filter(req => req.author.employeeID === store.employeeId);
    const executorRequests = requests.filter(req => req.executor?.employeeID === store.employeeId);
    const departmentRequests = requests.filter(req => req.equipment.departmentDTO.name === store.department);

    return (
        <div className="App">
            <div className="app-header">
                <h1>Добро пожаловать, {store.username} !</h1>
                <div>
                    <p>Тип пользователя: {store.userType}</p>
                    <p>Департамент: {store.department}</p>
                </div>

                <UserProfileDropdown
                    editUser={editUser}
                    logout={handleLogout}
                    isLoading={logoutLoading}
                    changePassword={handleChangePassword}
                />
            </div>

            <div className="app-content">
                <h2>Главная страница</h2>
                <div className="requests-section">
                    <div className="requests-header">
                        <h3>Список заявок</h3>
                        <div className="create-button-container">
                            <button
                                onClick={createRequest}
                                className="create-button"
                                disabled={requestsLoading}
                            >
                                Создать заявку
                            </button>
                            <button
                                onClick={handleRefreshRequests}
                                className="refresh-button"
                                disabled={requestsLoading}
                            >
                                {requestsLoading ? 'Загрузка...' : 'Обновить'}
                            </button>
                        </div>
                    </div>

                    {error && (
                        <div className="error-message">
                            {error}
                            <button
                                onClick={handleRefreshRequests}
                                className="retry-button"
                            >
                                Повторить
                            </button>
                        </div>
                    )}

                    {requestsLoading ? (
                        <div className="loading-requests">
                            <div className="loading-spinner small"></div>
                            <p>Загрузка заявок...</p>
                        </div>
                    ) : requests.length === 0 ? (
                        <div className="no-requests">
                            <p>Заявки не найдены</p>
                        </div>
                    ) : (
                        <div className="requests-container">
                            <div className="requests-list-header">
                                <div className="header-id">ID</div>
                                <div className="header-equipment">Оборудование</div>
                                <div className="header-problem">Описание проблемы</div>
                                <div className="header-author">Автор</div>
                                <div className="header-executor">Исполнитель</div>
                                <div className="header-dates">Даты</div>
                                <div className="header-status">Статус</div>
                            </div>

                            {userRequests.length > 0 && (
                                <div className="requests-category">
                                    <h3>Созданные заявки:</h3>
                                    <div className="requests-list">
                                        {userRequests.map((request) => (
                                            <RequestRow
                                                key={request.requestID}
                                                request={request}
                                                isHighlighted={highlightedRequests.has(request.requestID)}
                                                onClick={() => handleRequestClick(request)}
                                            />
                                        ))}
                                    </div>
                                </div>
                            )}

                            {executorRequests.length > 0 && (
                                <div className="requests-category">
                                    <h3>Заявки для исполнения:</h3>
                                    <div className="requests-list">
                                        {executorRequests.map((request) => (
                                            <RequestRow
                                                key={request.requestID}
                                                request={request}
                                                isHighlighted={highlightedRequests.has(request.requestID)}
                                                onClick={() => handleRequestClick(request)}
                                            />
                                        ))}
                                    </div>
                                </div>
                            )}

                            {departmentRequests.length > 0 && store.userType == 'HeadDepartment' && (
                                <div className="requests-category">
                                    <h3>Заявки отдела - {store.department}:</h3>
                                    <div className="requests-list">
                                        {departmentRequests.map((request) => (
                                            <RequestRow
                                                key={request.requestID}
                                                request={request}
                                                isHighlighted={highlightedRequests.has(request.requestID)}
                                                onClick={() =>  handleRequestClick(request)}
                                            />
                                        ))}
                                    </div>
                                </div>
                            )}
                        </div>
                    )}
                </div>
            </div>

            <CreateRequestModal
                isOpen={isCreateModalOpen}
                onClose={() => setIsCreateModalOpen(false)}
                onCreate={handleCreateRequest}
                requestTypes={requestTypes}
                equipmentList={equipmentList}
                employees={employees}
                isLoading={requestsLoading}
            />

            <UserEditModal
                user={currentUser}
                isOpen={isEditUserModalOpen}
                onClose={() => setIsEditUserModalOpen(false)}
                onSave={handleEditUser}
                isLoading={userLoading}
            />

            <EditRequestModal
                isOpen={isEditModalOpen}
                onClose={() => setIsEditModalOpen(false)}
                onUpdate={handleUpdateRequest}
                request={selectedRequest}
                requestTypes={requestTypes}
                equipmentList={equipmentList}
                employees={employees}
                isLoading={updateLoading}
            />

            {isChangePasswordModalOpen && (
                <ChangePasswordModal
                    isOpen={isChangePasswordModalOpen}
                    onClose={() => setIsChangePasswordModalOpen(false)}
                />
            )}
        </div>
    );
}

export default observer(App);