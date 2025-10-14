import React from 'react';
import { IRequest } from '../models/IRequest';
import './RequestCard.css';

interface RequestCardProps {
    request: IRequest;
    onClick?: (request: IRequest) => void;
}

const RequestCard: React.FC<RequestCardProps> = ({ request, onClick }) => {
    const formatDate = (dateString: Date | null): string => {
        if (!dateString) return 'Не указано';
        try {
            const date = new Date(dateString);
            return date.toLocaleString('ru-RU');
        } catch (error) {
            console.error('Invalid date format:', dateString);
            return 'Неверный формат даты';
        }
    };

    const getPriorityText = (priority: number): string => {
        switch (priority) {
            case 1: return 'Критический';
            case 2: return 'Высокий';
            case 3: return 'Средний';
            case 4: return 'Низкий';
            case 5: return 'Минимальный';
            default: return 'Неизвестно';
        }
    };

    const getPriorityClass = (priority: number): string => {
        switch (priority) {
            case 1: return 'priority-high';
            case 2: return 'priority-medium';
            case 3: return 'priority-low';
            default: return 'priority-unknown';
        }
    };

    const handleClick = () => {
        if (onClick) {
            onClick(request);
        }
    };

    return (
        <div className="request-card" onClick={handleClick}>
            <div className="request-card-header">
                <h4>Заявка #{request.requestID}</h4>
                <span className={`status-badge status-${request.requestStatus.statusID}`}>
                    {request.requestStatus.name}
                </span>
            </div>

            <div className="request-card-body">
                <div className="request-info">
                    <p><strong>Тип:</strong> {request.requestType.name}</p>
                    <p><strong>Приоритет:</strong>
                        <span className={`priority ${getPriorityClass(request.priority)}`}>
                            {getPriorityText(request.priority)}
                        </span>
                    </p>
                    <p><strong>Создана:</strong> {formatDate(new Date(request.creationDate))}</p>
                    <p><strong>Дедлайн:</strong> {formatDate(request.deadline ? new Date(request.deadline) : null)}</p>
                </div>

                <div className="problem-description">
                    <strong>Описание:</strong>
                    <p>{request.problemDescription}</p>
                </div>

                <div className="request-meta">
                    <p><strong>Автор:</strong> {request.author.fullName}</p>
                    <p><strong>Оборудование:</strong> {request.equipment.name}</p>
                    <p><strong>Исполнитель:</strong> {request.executor?.fullName || 'Не назначен'}</p>
                </div>
            </div>

            <div className="request-card-footer">
                <span className="creation-date">
                    Создана: {formatDate(new Date(request.creationDate))}
                </span>
            </div>
        </div>
    );
};

export default RequestCard;