import $api from "../http";
import { IEmployee, IEquipment, IRequest, IRequestStatus, IRequestType } from "../models/IRequest";
export default class RequestService {
    static fetchRequest() {
        return $api.get<IRequest[]>(`/request`, { withCredentials: true });
    }

    static fetchRequestTypes() {
        return $api.get<IRequestType[]>(`/requesttype`, { withCredentials: true });
    }

    static fetchRequestStatus() {
        return $api.get<IRequestStatus[]>(`/requeststatus`, { withCredentials: true });
    }

    static fetchEquipment() {
        return $api.get<IEquipment[]>(`/equipment`, { withCredentials: true });
    }

    static fetchEmployees() {
        return $api.get<IEmployee[]>(`/employee`, { withCredentials: true });
    }
    static createRequest(request: IRequest) {
        return $api.post(`/request`, request, { withCredentials: true });
    }

    static updateRequest(request: IRequest) {
        return $api.put(`/request`, request, { withCredentials: true });
    }

    static getEquipmentInstruction(id: number) {
        return $api.get<Blob>(`/equipment/GetVirtualFile/${id}`, { responseType: 'blob', withCredentials: true });
    }
}