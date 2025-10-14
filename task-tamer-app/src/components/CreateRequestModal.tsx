import React, { useState, useEffect } from 'react';
import { IRequest, IRequestType, IEquipment, IEmployee, IRequestStatus } from '../models/IRequest';
import "./CreateRequestModal.css";

interface CreateRequestModalProps {
    isOpen: boolean;
    onClose: () => void;
    onCreate: (requestData: IRequest) => void;
    requestTypes: IRequestType[];
    equipmentList: IEquipment[];
    employees: IEmployee[];
    isLoading?: boolean;
}

// Интерфейс для конфигурации приоритетов по типам заявок
interface RequestTypePriorityConfig {
    [key: string]: number; // название типа заявки -> приоритет
}

const CreateRequestModal: React.FC<CreateRequestModalProps> = ({
    isOpen,
    onClose,
    onCreate,
    requestTypes,
    equipmentList,
    employees,
    isLoading = false
}) => {
    const [formData, setFormData] = useState<IRequest>({
        requestID: 0,
        creationDate: new Date().toISOString(),
        author: {} as IEmployee,
        requestStatus: { statusID: 1, name: 'Новая', description: '', processingOrder: 1 } as IRequestStatus,
        requestType: { requestTypeID: 0 } as IRequestType,
        problemDescription: '',
        priority: 3,
        equipment: { equipmentID: 0 } as IEquipment,
        executor: {} as IEmployee,
        deadline: undefined,
        completionDate: undefined,
        history: []
    });

    const [errors, setErrors] = useState<Partial<{ [K in keyof IRequest]?: string }>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Конфигурация автоматического выбора приоритета на основе типа заявки
    const requestTypePriorityConfig: RequestTypePriorityConfig = {
        'Срочный ремонт': 1,
        'Ремонт': 2,
        'Замена': 2,
        'Техобслуживание': 3,
        'Осмотр': 4,
        'Апгрейд': 3,
        'Установка': 3,
        'Обучение': 4,
        'Консультация': 4,
        'Гарантийное обслуживание': 3
    };

    // Функция для получения приоритета по типу заявки
    const getPriorityByRequestType = (requestTypeName: string): number => {
        return requestTypePriorityConfig[requestTypeName] || 3; // по умолчанию средний приоритет
    };

    const findEquipmentResponsible = (equipment: IEquipment): IEmployee | undefined => {
        if (!equipment.responsibleEmployee.employeeID) return undefined;

        const responsibleEmployee = employees.find(emp =>
            emp.employeeID === equipment.responsibleEmployee.employeeID
        );

        return responsibleEmployee;
    };

    useEffect(() => {
        if (!isOpen) {
            setFormData({
                requestID: 0,
                creationDate: new Date().toISOString(),
                author: {} as IEmployee,
                requestStatus: { statusID: 1, name: 'Новая', description: '', processingOrder: 1 } as IRequestStatus,
                requestType: { requestTypeID: 0 } as IRequestType,
                problemDescription: '',
                priority: 3,
                equipment: { equipmentID: 0 } as IEquipment,
                executor: {} as IEmployee,
                deadline: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString().slice(0, 16),
                completionDate: undefined,
                history: []
            });
            setErrors({});
            setIsSubmitting(false);
        }
    }, [isOpen]);

    const validateForm = (): boolean => {
        const newErrors: Partial<{ [K in keyof IRequest]?: string }> = {};

        if (!formData.problemDescription.trim()) {
            newErrors.problemDescription = 'Описание проблемы обязательно';
        }

        if (formData.problemDescription.length < 10) {
            newErrors.problemDescription = 'Описание должно содержать минимум 10 символов';
        }

        if (formData.requestType.requestTypeID === 0) {
            newErrors.requestType = 'Выберите тип заявки';
        }

        if (formData.equipment.equipmentID === 0) {
            newErrors.equipment = 'Выберите оборудование';
        }

        if (formData.priority < 1 || formData.priority > 5) {
            newErrors.priority = 'Приоритет должен быть от 1 до 5';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (validateForm() && !isSubmitting) {
            setIsSubmitting(true);
            try {
                const submitData: IRequest = {
                    ...formData,
                    requestID: 0,
                    creationDate: new Date().toISOString(),
                    deadline: formData.deadline || undefined,
                    completionDate: undefined
                };

                await onCreate(submitData);
            } catch (error) {
                console.error('Ошибка при создании заявки:', error);
            } finally {
                setIsSubmitting(false);
            }
        }
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
        const { name, value } = e.target;

        setFormData(prev => ({
            ...prev,
            [name]: name === 'priority' ? parseInt(value) || 0 : value
        }));

        if (errors[name as keyof IRequest]) {
            setErrors(prev => ({ ...prev, [name]: undefined }));
        }
    };

    const handleRequestTypeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const requestTypeID = parseInt(e.target.value);
        const selectedType = requestTypes.find(type => type.requestTypeID === requestTypeID);

        if (selectedType) {
            // Автоматически устанавливаем приоритет на основе типа заявки
            const autoPriority = getPriorityByRequestType(selectedType.name);
            
            setFormData(prev => ({
                ...prev,
                requestType: selectedType,
                priority: autoPriority
            }));

            console.log(`Автоматически установлен приоритет ${autoPriority} для типа заявки "${selectedType.name}"`);
        } else {
            setFormData(prev => ({
                ...prev,
                requestType: { requestTypeID: 0 } as IRequestType,
                priority: 3 // сбрасываем к среднему приоритету
            }));
        }

        if (errors.requestType) {
            setErrors(prev => ({ ...prev, requestType: undefined }));
        }
    };

    const handleEquipmentChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const equipmentID = parseInt(e.target.value);
        const selectedEquipment = equipmentList.find(equip => equip.equipmentID === equipmentID);

        if (selectedEquipment) {
            // Находим ответственного за выбранное оборудование
            const responsibleEmployee = findEquipmentResponsible(selectedEquipment);

            setFormData(prev => ({
                ...prev,
                equipment: selectedEquipment,
                executor: responsibleEmployee || {} as IEmployee // Автоматически назначаем ответственного
            }));

            if (responsibleEmployee) {
                console.log(`Автоматически назначен ответственный: ${responsibleEmployee.fullName}`);
            }
        } else {
            setFormData(prev => ({
                ...prev,
                equipment: { equipmentID: 0 } as IEquipment,
                executor: {} as IEmployee
            }));
        }

        if (errors.equipment) {
            setErrors(prev => ({ ...prev, equipment: undefined }));
        }
    };

    const handleExecutorChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const employeeID = parseInt(e.target.value);
        const selectedEmployee = employees.find(emp => emp.employeeID === employeeID);

        setFormData(prev => ({
            ...prev,
            executor: selectedEmployee || {} as IEmployee
        }));
    };

    const handleDeadlineChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { value } = e.target;
        setFormData(prev => ({
            ...prev,
            deadline: value || undefined
        }));
    };

    // Функция для получения названия приоритета
    const getPriorityName = (priority: number): string => {
        const priorityNames: { [key: number]: string } = {
            1: 'Критический',
            2: 'Высокий',
            3: 'Средний',
            4: 'Низкий',
            5: 'Минимальный'
        };
        return priorityNames[priority] || 'Средний';
    };

    if (!isOpen) return null;

    return (
        <div className="crm-modal-overlay">
            <div className="crm-modal-content">
                <div className="crm-modal-header">
                    <h2>Создание новой заявки</h2>
                    <button
                        className="crm-modal-close"
                        onClick={onClose}
                        disabled={isLoading || isSubmitting}
                    >
                        ×
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="crm-request-form">
                    <div className="crm-form-group">
                        <label htmlFor="problemDescription">Описание проблемы *</label>
                        <textarea
                            id="problemDescription"
                            name="problemDescription"
                            value={formData.problemDescription}
                            onChange={handleInputChange}
                            rows={4}
                            placeholder="Опишите подробно возникшую проблему..."
                            className={errors.problemDescription ? 'crm-error' : ''}
                            disabled={isLoading || isSubmitting}
                        />
                        {errors.problemDescription && (
                            <span className="crm-error-text">{errors.problemDescription}</span>
                        )}
                    </div>

                    <div className="crm-form-row">
                        <div className="crm-form-group">
                            <label htmlFor="requestType">Тип заявки *</label>
                            <select
                                id="requestType"
                                value={formData.requestType.requestTypeID}
                                onChange={handleRequestTypeChange}
                                className={errors.requestType ? 'crm-error' : ''}
                                disabled={isLoading || isSubmitting}
                            >
                                <option value={0}>Выберите тип заявки</option>
                                {requestTypes.map(type => (
                                    <option key={type.requestTypeID} value={type.requestTypeID}>
                                        {type.name}
                                    </option>
                                ))}
                            </select>
                            {errors.requestType && (
                                <span className="crm-error-text">{errors.requestType}</span>
                            )}
                        </div>

                        <div className="crm-form-group">
                            <label htmlFor="priority">Приоритет *</label>
                            <select
                                id="priority"
                                name="priority"
                                value={formData.priority}
                                onChange={handleInputChange}
                                className={errors.priority ? 'crm-error' : ''}
                                disabled={true}
                            >
                                <option value={1}>Критический (1)</option>
                                <option value={2}>Высокий (2)</option>
                                <option value={3}>Средний (3)</option>
                                <option value={4}>Низкий (4)</option>
                                <option value={5}>Минимальный (5)</option>
                            </select>
                            {errors.priority && (
                                <span className="crm-error-text">{errors.priority}</span>
                            )}
                            {formData.requestType.requestTypeID !== 0 && (
                                <div className="crm-priority-info">
                                    Автоматически установлен {getPriorityName(formData.priority)} приоритет 
                                    для типа "{formData.requestType.name}"
                                </div>
                            )}
                        </div>
                    </div>

                    <div className="crm-form-row">
                        <div className="crm-form-group">
                            <label htmlFor="equipment">Оборудование *</label>
                            <select
                                id="equipment"
                                value={formData.equipment.equipmentID}
                                onChange={handleEquipmentChange}
                                className={errors.equipment ? 'crm-error' : ''}
                                disabled={isLoading || isSubmitting}
                            >
                                <option value={0}>Выберите оборудование</option>
                                {equipmentList.map(equipment => (
                                    <option key={equipment.equipmentID} value={equipment.equipmentID}>
                                        {equipment.name} - {equipment.model} ({equipment.serialNumber})
                                        {equipment.responsibleEmployee.employeeID && ' (Есть ответственный)'}
                                    </option>
                                ))}
                            </select>
                            {errors.equipment && (
                                <span className="crm-error-text">{errors.equipment}</span>
                            )}
                        </div>

                        <div className="crm-form-group">
                            <label htmlFor="executor">Исполнитель</label>
                            <select
                                id="executor"
                                value={formData.executor?.employeeID || ''}
                                onChange={handleExecutorChange}
                                disabled={true}
                            >
                                <option value="">Не назначен</option>
                                {employees.map(employee => (
                                    <option key={employee.employeeID} value={employee.employeeID}>
                                        {employee.fullName} ({employee.positionDTO.title})
                                    </option>
                                ))}
                            </select>
                            {formData.executor?.employeeID && formData.equipment?.equipmentID !== 0 && (
                                <div className="crm-executor-info">
                                    {formData.executor.employeeID === formData.equipment.responsibleEmployee.employeeID ?
                                        `Автоматически выбран ответственный за оборудование` :
                                        'Исполнитель назначен вручную'
                                    }
                                </div>
                            )}
                        </div>
                    </div>

                    <div className="crm-form-group">
                        <label htmlFor="deadline">Срок выполнения</label>
                        <input
                            type="datetime-local"
                            id="deadline"
                            name="deadline"
                            value={formData.deadline}
                            readOnly
                            disabled={true}
                        />
                        <div className="crm-executor-info">
                            Автоматически выбрана дата выполнения работы( Руководетель может изменить дату выполнения заявки при назначение исполнителя)
                        </div>
                    </div>

                    <div className="crm-modal-actions">
                        <button
                            type="button"
                            onClick={onClose}
                            className="crm-btn-secondary"
                            disabled={isLoading || isSubmitting}
                        >
                            Отмена
                        </button>
                        <button
                            type="submit"
                            className="crm-btn-primary"
                            disabled={isLoading || isSubmitting}
                        >
                            {isSubmitting ? 'Создание...' : 'Создать заявку'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default CreateRequestModal;