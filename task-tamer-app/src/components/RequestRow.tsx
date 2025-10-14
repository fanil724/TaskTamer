import React from 'react';
import { IRequest } from '../models/IRequest';
import './RequestRow.css';

interface RequestRowProps {
    request: IRequest;
    onClick?: (request: IRequest) => void;
     isHighlighted?: boolean;
}

const RequestRow: React.FC<RequestRowProps> = ({ request, onClick, isHighlighted = false }) => {
    const formatDate = (date: Date | null): string => {
        if (!date) return '—';
        return new Date(date).toLocaleDateString('ru-RU');
    };

    const getPriorityIcon = (priority: number): string => {
        switch (priority) {
            case 1: return '🔴';
            case 2: return '🟠';
            case 3: return '🟡';
            case 4: return '🟢';
            case 5: return '⚪';
            default: return '❓';
        }
    };

    const getStatusColor = (statusID: number): string => {
        switch (statusID) {
            case 1: return 'status-new';
            case 2: return 'status-in-progress';
            case 3: return 'status-completed';
            case 4: return 'status-rejected';
            default: return 'status-unknown';
        }
    };

    const handleClick = () => onClick?.(request);

    return (
        <div  className={`request-row ${isHighlighted ? 'highlighted' : ''}`} onClick={handleClick}>
            <div className="row-main">
                <div className="row-id-priority">
                    <span className="priority-icon">{getPriorityIcon(request.priority)}</span>
                    <span className="request-id">#{request.requestID}</span>
                </div>

                <div className="row-equipment">
                    <span className="equipment-name">{request.equipment.name}</span>
                </div>

                <div className="row-problem">
                    <span className="problem-text" title={request.problemDescription}>
                        {request.problemDescription.length > 60
                            ? `${request.problemDescription.substring(0, 60)}...`
                            : request.problemDescription
                        }
                    </span>
                </div>

                <div className="row-author">
                    <span className="author" title="Автор">
                        👤 {request.author.fullName.split(' ')[0]}
                    </span>
                </div>

                <div className="row-executor">
                    <span className="executor" title="Исполнитель">
                        🛠️ {request.executor?.fullName?.split(' ')[0] || '—'}
                    </span>
                </div>

                <div className="row-dates">
                    <span className="creation-date" title="Дата создания">
                        📅 {formatDate(new Date(request.creationDate))}
                    </span>
                    <span className="deadline" title="Дедлайн">
                        ⏰ {formatDate(request.deadline ? new Date(request.deadline) : null)}
                    </span>
                </div>

                <div className="row-status">
                    <span className={`status ${getStatusColor(request.requestStatus.statusID)}`}>
                        {request.requestStatus.name}
                    </span>
                </div>
            </div>
        </div>
    );
};

export default RequestRow;