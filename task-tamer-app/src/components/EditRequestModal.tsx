import React, { useState, useEffect, useContext, useCallback, useMemo } from 'react';
import { IRequest, IRequestType, IEquipment, IEmployee, IRequestStatus, IHistory } from "../models/IRequest";
import RequestService from "../services/RequestService";
import { Context } from "../index";
import "./EditRequestModal.css"

interface EditRequestModalProps {
    isOpen: boolean;
    onClose: () => void;
    onUpdate: (requestData: IRequest) => void;
    request: IRequest | null;
    requestTypes: IRequestType[];
    equipmentList: IEquipment[];
    employees: IEmployee[];
    isLoading: boolean;
}

const EditRequestModal: React.FC<EditRequestModalProps> = ({
                                                               isOpen,
                                                               onClose,
                                                               onUpdate,
                                                               request,
                                                               requestTypes,
                                                               equipmentList,
                                                               employees,
                                                               isLoading
                                                           }) => {
    const { store } = useContext(Context);
    const [formData, setFormData] = useState<IRequest>({
        requestID: 0,
        requestType: { requestTypeID: 0, name: '', processingOrder: 0 },
        equipment: {
            equipmentID: 0,
            name: '',
            technicalDocumentation: '',
            serialNumber: '',
            departmentDTO: {
                departmentID: 0,
                name: '',
                description: '',
                departmentType: '',
                creationDate: '',
                isActive: true
            },
            location: '',
            purchaseDate: '',
            manufacturer: '',
            type: '',
            responsibleEmployee: {
                employeeID: 0,
                fullName: '',
                positionDTO: { positionID: 0, title: '', description: '', accessLevel: 0 },
                departmentDTO: {
                    departmentID: 0,
                    name: '',
                    description: '',
                    departmentType: '',
                    creationDate: '',
                    isActive: true
                },
                phone: '',
                email: '',
                userType: 'Employee',
                registrationDate: '',
                terminationDate: null,
                isActive: true
            },
            model: ''
        },
        problemDescription: '',
        priority: 3,
        author: {
            employeeID: 0,
            fullName: '',
            positionDTO: { positionID: 0, title: '', description: '', accessLevel: 0 },
            departmentDTO: {
                departmentID: 0,
                name: '',
                description: '',
                departmentType: '',
                creationDate: '',
                isActive: true
            },
            phone: '',
            email: '',
            userType: 'Employee',
            registrationDate: '',
            terminationDate: null,
            isActive: true
        },
        creationDate: '',
        requestStatus: { statusID: 0, name: '', description: '', processingOrder: 0 },
        completionDate: undefined,
        deadline: undefined,
        executor: {
            employeeID: 0,
            fullName: '',
            positionDTO: { positionID: 0, title: '', description: '', accessLevel: 0 },
            departmentDTO: {
                departmentID: 0,
                name: '',
                description: '',
                departmentType: '',
                creationDate: '',
                isActive: true
            },
            phone: '',
            email: '',
            userType: 'Employee',
            registrationDate: '',
            terminationDate: null,
            isActive: true
        },
        history: []
    });

    const [errors, setErrors] = useState<{ [key: string]: string }>({});
    const [statusOptions, setStatusOptions] = useState<IRequestStatus[]>([]);
    const [isLoadingStatuses, setIsLoadingStatuses] = useState(false);
    const [activeTab, setActiveTab] = useState<'edit' | 'history'>('edit');
    const [newDescription, setNewDescription] = useState("");

    const [showStatusConfirmation, setShowStatusConfirmation] = useState(false);
    const [nextStatus, setNextStatus] = useState<IRequestStatus | null>(null);
    const [pendingStatusChange, setPendingStatusChange] = useState<IRequestStatus | null>(null);


    const [originalData, setOriginalData] = useState<IRequest | null>(null);

    useEffect(() => {
        if (!isOpen) {
            setPendingStatusChange(null);
            setShowStatusConfirmation(false);
            setNextStatus(null);
            setOriginalData(null);
        }
    }, [isOpen]);

    useEffect(() => {
        if (request) {
            setFormData(request);
            setOriginalData(request);
            setPendingStatusChange(null);
        }
    }, [request]);

    useEffect(() => {
        if (isOpen) {
            fetchRequestStatuses();
        }
    }, [isOpen]);

    const fetchRequestStatuses = async () => {
        setIsLoadingStatuses(true);
        try {
            const response = await RequestService.fetchRequestStatus();
            if (response.data) {
                setStatusOptions(response.data);
            }
        } catch (error) {
            console.error('Ошибка при загрузке статусов заявок:', error);
        } finally {
            setIsLoadingStatuses(false);
        }
    };

    const getAvailableExecutors = (): IEmployee[] => {
        if (!formData.equipment?.departmentDTO?.departmentID) {
            return [];
        }

        return employees.filter(employee =>
            employee.departmentDTO?.departmentID === formData.equipment.departmentDTO.departmentID &&
            employee.isActive
        );
    };

    const availableExecutors = getAvailableExecutors();

    const isAuthor = store.employeeId === request?.author.employeeID;
    const isExecutor = store.employeeId === request?.executor?.employeeID;
    const isDepartmentManager = (store.role === 'Manager' && store.department === formData.equipment.departmentDTO.name) || store.role === 'Admin';
    const isAdmin = store.role === 'Admin';

    const canAssignExecutor = isDepartmentManager && formData.requestStatus.name === 'Создана';
    const canEditRequest = isAuthor || isExecutor || isDepartmentManager || isAdmin;
    const canEditDeadline = canAssignExecutor && formData.requestStatus.name === 'Создана';


    const hasChanges = useCallback((): boolean => {
        if (!originalData) return false;

        if (pendingStatusChange) {
            return true;
        }

        const currentExecutorId = formData.executor?.employeeID || 0;
        const originalExecutorId = originalData.executor?.employeeID || 0;
        if (currentExecutorId !== originalExecutorId) {
            return true;
        }

        const currentDeadline = formData.deadline ? new Date(formData.deadline).getTime() : null;
        const originalDeadline = originalData.deadline ? new Date(originalData.deadline).getTime() : null;
        if (currentDeadline !== originalDeadline) {
            return true;
        }

        if (formData.problemDescription !== originalData.problemDescription) {
            return true;
        }

        return false;
    }, [formData, originalData, pendingStatusChange]);

    const hasUnsavedChanges = useMemo(() => hasChanges(), [hasChanges]);

    const getNextStatus = useCallback((): IRequestStatus | null => {
        const currentStatus = pendingStatusChange?.name || formData.requestStatus.name;

        if (!canEditRequest) return null;

        if (isAuthor) {
            if (currentStatus === 'Ожидает проверки') {
                return statusOptions.find(s => s.name === 'Завершена') || null;
            }
            if (currentStatus === 'Завершена') {
                return statusOptions.find(s => s.name === 'Переоткрыта') || null;
            }
            if (currentStatus !== 'Завершена' && currentStatus !== 'Отменена') {
                return statusOptions.find(s => s.name === 'Отменена') || null;
            }
        }

        if (isExecutor) {
            if (currentStatus === 'Назначена') {
                return statusOptions.find(s => s.name === 'В работе') || null;
            }
            if (currentStatus === 'В работе') {
                return statusOptions.find(s => s.name === 'Ожидает проверки') || null;
            }
        }

        if (isDepartmentManager || isAdmin) {
            if (currentStatus === 'Создана') {
                return statusOptions.find(s => s.name === 'Назначена') || null;
            }
            if (currentStatus === 'В работе') {
                return statusOptions.find(s => s.name === 'Приостановлена') || null;
            }
            if (currentStatus === 'Приостановлена') {
                return statusOptions.find(s => s.name === 'В работе') || null;
            }
        }

        return null;
    }, [formData.requestStatus.name, pendingStatusChange, canEditRequest, isAuthor, isExecutor, isDepartmentManager, isAdmin, statusOptions]);

    const nextAvailableStatus = useMemo(() => getNextStatus(), [getNextStatus]);

    const getNextStepButtonText = useCallback((): string => {
        if (!nextAvailableStatus) return "Нет доступных действий";

        const currentStatus = pendingStatusChange?.name || formData.requestStatus.name;
        const nextStatusName = nextAvailableStatus.name;

        const statusActions: { [key: string]: { [key: string]: string } } = {
            author: {
                'Ожидает проверки': 'Завершить заявку',
                'Завершена': 'Переоткрыть заявку',
                'default': 'Отменить заявку'
            },
            executor: {
                'Назначена': 'Принять в работу',
                'В работе': 'Отправить на проверку'
            },
            manager: {
                'Создана': 'Назначить исполнителя',
                'В работе': 'Приостановить',
                'Приостановлена': 'Возобновить работу'
            }
        };

        if (isAuthor) {
            return statusActions.author[currentStatus] || statusActions.author.default;
        }
        if (isExecutor) {
            return statusActions.executor[currentStatus] || `Перевести в "${nextStatusName}"`;
        }
        if (isDepartmentManager || isAdmin) {
            return statusActions.manager[currentStatus] || `Перевести в "${nextStatusName}"`;
        }

        return `Перевести в "${nextStatusName}"`;
    }, [nextAvailableStatus, formData.requestStatus.name, pendingStatusChange, isAuthor, isExecutor, isDepartmentManager, isAdmin]);

    const handleNextStepClick = () => {
        if (!nextAvailableStatus) return;

        setNextStatus(nextAvailableStatus);
        setShowStatusConfirmation(true);
    };

    const handleStatusConfirm = () => {
        if (!nextStatus) return;

        setPendingStatusChange(nextStatus);
        setShowStatusConfirmation(false);
        setNextStatus(null);
    };

    const handleStatusCancel = () => {
        setShowStatusConfirmation(false);
        setNextStatus(null);
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;

        if (name === 'priority') {
            setFormData(prev => ({
                ...prev,
                [name]: parseInt(value, 10)
            }));
        } else {
            setFormData(prev => ({
                ...prev,
                [name]: value
            }));
        }

        if (errors[name]) {
            setErrors(prev => {
                const newErrors = { ...prev };
                delete newErrors[name];
                return newErrors;
            });
        }
    };

    const handleSelectChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const { name, value } = e.target;
        const numericValue = parseInt(value, 10);

        if (name === 'requestType.requestTypeID') {
            const selectedType = requestTypes.find(type => type.requestTypeID === numericValue);
            setFormData(prev => ({
                ...prev,
                requestType: selectedType || { requestTypeID: 0, name: '', processingOrder: 0 }
            }));
        } else if (name === 'equipment.equipmentID') {
            const selectedEquipment = equipmentList.find(equip => equip.equipmentID === numericValue);
            setFormData(prev => ({
                ...prev,
                equipment: selectedEquipment || {
                    equipmentID: 0,
                    name: '',
                    technicalDocumentation: '',
                    serialNumber: '',
                    departmentDTO: {
                        departmentID: 0,
                        name: '',
                        description: '',
                        departmentType: '',
                        creationDate: '',
                        isActive: true
                    },
                    location: '',
                    purchaseDate: '',
                    manufacturer: '',
                    type: '',
                    responsibleEmployee: {
                        employeeID: 0,
                        fullName: '',
                        positionDTO: { positionID: 0, title: '', description: '', accessLevel: 0 },
                        departmentDTO: {
                            departmentID: 0,
                            name: '',
                            description: '',
                            departmentType: '',
                            creationDate: '',
                            isActive: true
                        },
                        phone: '',
                        email: '',
                        userType: 'Employee',
                        registrationDate: '',
                        terminationDate: null,
                        isActive: true
                    },
                    model: ''
                }
            }));
        } else if (name === 'executor.employeeID') {
            const selectedExecutor = availableExecutors.find(emp => emp.employeeID === numericValue);
            setFormData(prev => ({
                ...prev,
                executor: selectedExecutor || {
                    employeeID: 0,
                    fullName: '',
                    positionDTO: { positionID: 0, title: '', description: '', accessLevel: 0 },
                    departmentDTO: {
                        departmentID: 0,
                        name: '',
                        description: '',
                        departmentType: '',
                        creationDate: '',
                        isActive: true
                    },
                    phone: '',
                    email: '',
                    userType: 'Employee',
                    registrationDate: '',
                    terminationDate: null,
                    isActive: true
                }
            }));
        }
    };

    const handleDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value ? new Date(value).toISOString() : null
        }));
    };

    const handleDownloadManual = async () => {
        if (formData.equipment.technicalDocumentation) {
            try {
                const response = await RequestService.getEquipmentInstruction(
                    formData.equipment.equipmentID
                );
                const blob = new Blob([response.data]);
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = `Инструкция_${formData.equipment.name}`;
                link.target = '_blank';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                window.URL.revokeObjectURL(url);
            } catch (error) {
                if (error instanceof Error) {
                    console.error('Ошибка при скачивании файла:', error.message);
                }

                console.error('Ошибка при скачивании файла:', error);
                alert('Не удалось скачать инструкцию');
            }
        }
    };

    const validateForm = (): boolean => {
        const newErrors: { [key: string]: string } = {};

        if (!formData.requestType.requestTypeID) {
            newErrors['requestType'] = 'Тип заявки обязателен';
        }
        if (!formData.equipment.equipmentID) {
            newErrors['equipment'] = 'Оборудование обязательно';
        }
        if (!formData.problemDescription.trim()) {
            newErrors['problemDescription'] = 'Описание проблемы обязательно';
        }
        if (!formData.priority || formData.priority < 1 || formData.priority > 5) {
            newErrors['priority'] = 'Приоритет обязателен';
        }
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!validateForm()) return;

        let updatedData = { ...formData };

        if (pendingStatusChange) {
            updatedData = {
                ...updatedData,
                requestStatus: pendingStatusChange
            };

            if (pendingStatusChange.name === 'Завершена') {
                updatedData.completionDate = new Date().toISOString();
            }

            if (pendingStatusChange.name === 'Назначена' && formData.executor) {
                console.log('Заявка назначена исполнителю:', formData.executor.fullName);
            }
        }

        try {
            await onUpdate(updatedData);
            setPendingStatusChange(null);
            // Обновляем оригинальные данные после успешного сохранения
            setOriginalData(updatedData);
        } catch (error) {
            console.error('Ошибка при сохранении заявки:', error);
        }
    };

    const handleAddNewDescription = () => {
        if (!newDescription.trim()) return;

        const updatedDescription = formData.problemDescription
            ? `${formData.problemDescription}, ${newDescription}`
            : newDescription;

        setFormData(prev => ({
            ...prev,
            problemDescription: updatedDescription
        }));
        setNewDescription("");
    };

    const handleCancel = () => {
        if (hasUnsavedChanges) {
            const confirmClose = window.confirm(
                'У вас есть несохраненные изменения. Вы уверены, что хотите закрыть без сохранения?'
            );
            if (!confirmClose) {
                return;
            }
        }

        setPendingStatusChange(null);
        onClose();
    };

    const displayStatus = pendingStatusChange || formData.requestStatus;

    if (!isOpen || !request) return null;

    return (
        <div className="erm-modal-overlay">
            <div className="erm-modal-content">
                <div className="erm-modal-header">
                    <h2>Заявка #{request.requestID}</h2>
                    <button className="erm-modal-close" onClick={handleCancel}>×</button>
                </div>

                {hasUnsavedChanges && (
                    <div className="erm-unsaved-changes-warning">
                        ⚠ Есть несохраненные изменения. Нажмите "Сохранить изменения" для применения.
                    </div>
                )}

                <div className="erm-tabs">
                    <button
                        className={`erm-tab ${activeTab === 'edit' ? 'erm-tab-active' : ''}`}
                        onClick={() => setActiveTab('edit')}
                    >
                        Данные заявки
                    </button>
                    <button
                        className={`erm-tab ${activeTab === 'history' ? 'erm-tab-active' : ''}`}
                        onClick={() => setActiveTab('history')}
                    >
                        История изменений ({formData.history.length})
                    </button>
                </div>

                {activeTab === 'edit' ? (
                    <form onSubmit={handleSubmit} className="erm-request-form">
                        <div className="erm-form-row">
                            <div className="erm-form-group">
                                <label htmlFor="erm-requestType">Тип заявки *</label>
                                <select
                                    id="erm-requestType"
                                    name="requestType.requestTypeID"
                                    value={formData.requestType.requestTypeID}
                                    onChange={handleSelectChange}
                                    className={errors.requestType ? 'erm-error' : ''}
                                    disabled={true}
                                >
                                    <option value="">Выберите тип заявки</option>
                                    {requestTypes.map(type => (
                                        <option key={type.requestTypeID} value={type.requestTypeID}>
                                            {type.name}
                                        </option>
                                    ))}
                                </select>
                                {errors.requestType && <span className="erm-error-text">{errors.requestType}</span>}
                            </div>

                            <div className="erm-form-group">
                                <label htmlFor="erm-equipment">Оборудование *</label>
                                <select
                                    id="erm-equipment"
                                    name="equipment.equipmentID"
                                    value={formData.equipment.equipmentID}
                                    onChange={handleSelectChange}
                                    className={errors.equipment ? 'erm-error' : ''}
                                    disabled={true}
                                >
                                    <option value="">Выберите оборудование</option>
                                    {equipmentList.map(equip => (
                                        <option key={equip.equipmentID} value={equip.equipmentID}>
                                            {equip.name} ({equip.serialNumber})
                                        </option>
                                    ))}
                                </select>
                                {errors.equipment && <span className="erm-error-text">{errors.equipment}</span>}
                            </div>
                        </div>

                        <div className="erm-form-row">
                            <div className="erm-form-group">
                                <label htmlFor="erm-priority">Приоритет *</label>
                                <select
                                    id="erm-priority"
                                    name="priority"
                                    value={formData.priority}
                                    onChange={handleInputChange}
                                    className={errors.priority ? 'erm-error' : ''}
                                    disabled={true}
                                >
                                    <option value={1}>Критический</option>
                                    <option value={2}>Высокий</option>
                                    <option value={3}>Средний</option>
                                    <option value={4}>Низкий</option>
                                    <option value={5}>Минимальный</option>
                                </select>
                                {errors.priority && <span className="erm-error-text">{errors.priority}</span>}
                            </div>

                            <div className="erm-form-group">
                                <label>Текущий статус</label>
                                <div className="erm-current-status">
                                    <span className={`erm-status-badge erm-status-${displayStatus.name.toLowerCase().replace(' ', '-')}`}>
                                        {displayStatus.name}
                                        {pendingStatusChange && (
                                            <span className="erm-pending-indicator"> (ожидает сохранения)</span>
                                        )}
                                    </span>
                                </div>

                                {nextAvailableStatus && (
                                    <div className="erm-next-step">
                                        <button
                                            type="button"
                                            className="erm-next-step-btn"
                                            onClick={handleNextStepClick}
                                            disabled={isLoading || isLoadingStatuses || pendingStatusChange !== null}
                                        >
                                            {getNextStepButtonText()}
                                        </button>
                                        <span className="erm-next-step-info">
                                            Следующий статус: {nextAvailableStatus.name}
                                        </span>
                                    </div>
                                )}

                                {!nextAvailableStatus && canEditRequest && (
                                    <span className="erm-info-text">Нет доступных действий для этого статуса</span>
                                )}

                                {!canEditRequest && (
                                    <span className="erm-info-text">У вас нет прав для изменения статуса</span>
                                )}

                                {pendingStatusChange && (
                                    <div className="erm-pending-change">
                                        <span className="erm-warning-text">
                                            ⚠ Статус будет изменен на "{pendingStatusChange.name}" после сохранения
                                        </span>
                                    </div>
                                )}
                            </div>
                        </div>

                        <div className="erm-form-row">
                            <div className="erm-form-group">
                                <label htmlFor="erm-executor">Исполнитель</label>
                                <select
                                    id="erm-executor"
                                    name="executor.employeeID"
                                    value={formData.executor?.employeeID || ''}
                                    onChange={handleSelectChange}
                                    disabled={isLoading || !canAssignExecutor || availableExecutors.length === 0}
                                >
                                    <option value="">Не назначен</option>
                                    {availableExecutors.map(employee => (
                                        <option key={employee.employeeID} value={employee.employeeID}>
                                            {employee.fullName} ({employee.positionDTO.title})
                                        </option>
                                    ))}
                                </select>
                                {!canAssignExecutor && (
                                    <span className="erm-info-text">Назначать исполнителя может только руководитель отдела при статусе "Создана"</span>
                                )}
                                {canAssignExecutor && availableExecutors.length === 0 && (
                                    <span className="erm-warning-text">
                                        В отделе "{formData.equipment.departmentDTO.name}" нет активных сотрудников
                                    </span>
                                )}
                                {canAssignExecutor && availableExecutors.length > 0 && (
                                    <span className="erm-info-text">
                                        Доступные сотрудники отдела "{formData.equipment.departmentDTO.name}"
                                    </span>
                                )}
                            </div>

                            <div className="erm-form-group">
                                <label htmlFor="erm-deadline">Дедлайн</label>
                                <input
                                    type="datetime-local"
                                    id="erm-deadline"
                                    name="deadline"
                                    value={formData.deadline ? new Date(formData.deadline).toISOString().slice(0, 16) : ''}
                                    onChange={handleDateChange}
                                    disabled={isLoading || !canEditDeadline}
                                />
                                {!canEditDeadline && (
                                    <span className="erm-info-text">
                                        {formData.requestStatus.name === 'Создана'
                                            ? 'Изменять дедлайн может только руководитель отдела при статусе "Создана"'
                                            : 'Изменение дедлайна доступно только при статусе "Создана"'
                                        }
                                    </span>
                                )}
                            </div>
                        </div>

                        {formData.equipment.technicalDocumentation && (
                            <div className="erm-form-group">
                                <label>Техническая документация</label>
                                <div className="erm-documentation-link">
                                    <button
                                        type="button"
                                        className="erm-download-button"
                                        onClick={handleDownloadManual}
                                        disabled={isLoading}
                                    >
                                        📄 Скачать инструкцию для {formData.equipment.name}
                                    </button>
                                </div>
                            </div>
                        )}

                        <div className="erm-form-group">
                            <label htmlFor="erm-problemDescription">Описание проблемы *</label>
                            <textarea
                                id="erm-problemDescription"
                                name="problemDescription"
                                value={formData.problemDescription}
                                onChange={handleInputChange}
                                className={errors.problemDescription ? 'erm-error' : ''}
                                disabled={isLoading}
                                placeholder="Опишите подробно проблему..."
                                rows={4}
                            />
                            {errors.problemDescription && <span className="erm-error-text">{errors.problemDescription}</span>}
                        </div>

                        {(isAuthor || isExecutor) && (
                            <div className="erm-form-group">
                                <label htmlFor="erm-history-comment">Добавить описание проблемы</label>
                                <div className="erm-history-add">
                                    <textarea
                                        id="erm-history-comment"
                                        value={newDescription}
                                        onChange={(e) => setNewDescription(e.target.value)}
                                        placeholder="Дополните описание проблемы..."
                                        rows={2}
                                        disabled={isLoading}
                                    />
                                    <button
                                        type="button"
                                        className="erm-btn erm-btn-outline"
                                        onClick={handleAddNewDescription}
                                        disabled={isLoading || !newDescription.trim()}
                                    >
                                        Добавить описание
                                    </button>
                                </div>
                            </div>
                        )}

                        <div className="erm-user-permissions-info">
                            <h4>Ваши права:</h4>
                            <ul>
                                {isAuthor && <li>✓ Вы автор заявки - можете завершать, переоткрывать и отменять</li>}
                                {isExecutor && <li>✓ Вы исполнитель - можете принимать в работу и отправлять на проверку</li>}
                                {isDepartmentManager && <li>✓ Вы руководитель отдела - можете назначать исполнителей и изменять дедлайн (только при статусе "Создана")</li>}
                            </ul>
                        </div>

                        <div className="erm-modal-actions">
                            <button
                                type="button"
                                className="erm-btn erm-btn-secondary"
                                onClick={handleCancel}
                                disabled={isLoading}
                            >
                                Отмена
                            </button>
                            <button
                                type="submit"
                                className="erm-btn erm-btn-primary"
                                disabled={isLoading || isLoadingStatuses || !canEditRequest || !hasUnsavedChanges}
                            >
                                {isLoading ? 'Сохранение...' : 'Сохранить изменения'}
                            </button>
                        </div>
                    </form>
                ) : (
                    <div className="erm-history-tab">
                        <div className="erm-history-list">
                            {formData.history.length === 0 ? (
                                <div className="erm-no-history">
                                    <p>История изменений отсутствует</p>
                                </div>
                            ) : (
                                formData.history
                                    .sort((a, b) => new Date(b.changeDate).getTime() - new Date(a.changeDate).getTime())
                                    .map((historyItem) => (
                                        <div key={historyItem.historyID} className="erm-history-item">
                                            <div className="erm-history-header">
                                                <span className="erm-history-date">
                                                    {new Date(historyItem.changeDate).toLocaleString('ru-RU')}
                                                </span>
                                                <span className="erm-history-status">
                                                    Статус: {historyItem.status.name}
                                                </span>
                                            </div>
                                            <div className="erm-history-user">
                                                Изменил: {historyItem.changedBy.fullName}
                                                ({historyItem.changedBy.positionDTO.title})
                                            </div>
                                            {historyItem.comment && (
                                                <div className="erm-history-comment">
                                                    <strong>Комментарий:</strong> {historyItem.comment}
                                                </div>
                                            )}
                                        </div>
                                    ))
                            )}
                        </div>
                    </div>
                )}

                {/* Модальное окно подтверждения изменения статуса */}
                {showStatusConfirmation && nextStatus && (
                    <div className="erm-confirmation-overlay">
                        <div className="erm-confirmation-modal">
                            <div className="erm-confirmation-header">
                                <h3>Подтверждение действия</h3>
                            </div>
                            <div className="erm-confirmation-body">
                                <p>
                                    Вы уверены, что хотите <strong>{getNextStepButtonText().toLowerCase()}</strong>?
                                </p>
                                <p>
                                    Статус заявки изменится с "<strong>{displayStatus.name}</strong>" на
                                    "<strong>{nextStatus.name}</strong>"
                                </p>
                            </div>
                            <div className="erm-confirmation-actions">
                                <button
                                    type="button"
                                    className="erm-btn erm-btn-secondary"
                                    onClick={handleStatusCancel}
                                >
                                    Отмена
                                </button>
                                <button
                                    type="button"
                                    className="erm-btn erm-btn-primary"
                                    onClick={handleStatusConfirm}
                                >
                                    Подтвердить
                                </button>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default React.memo(EditRequestModal);