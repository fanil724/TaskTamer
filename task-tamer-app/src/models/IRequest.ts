
export interface IRequest {
    requestID: number,
    creationDate: string,
    author: IEmployee,
    requestStatus: IRequestStatus,
    requestType: IRequestType,
    problemDescription: string,
    priority: number,
    equipment: IEquipment,
    executor: IEmployee,
    deadline?: string,
    completionDate?: string,
    history: IHistory[]
}

export interface IRequestType {
    requestTypeID: number,
    name: string,
    processingOrder: number
}

export interface IEquipment {
    equipmentID: number,
    name: string,
    model: string,
    serialNumber: string,
    type: string,
    manufacturer: string,
    purchaseDate: string,
    responsibleEmployee: IEmployee,
    departmentDTO: IDepartment,
    location: string,
    technicalDocumentation: string
}

export interface IEmployee {
    employeeID: number;
    fullName: string;
    positionDTO: IPosition;
    departmentDTO: IDepartment;
    phone: string;
    email: string;
    userType: string;
    registrationDate: string;
    terminationDate: string | null;
    isActive: boolean;
}

export interface IRequestStatus {
    statusID: number,
    name: string,
    description: string,
    processingOrder: number
}

export interface IDepartment {
    departmentID: number,
    name: string,
    description: string,
    departmentType: string,
    creationDate: string,
    isActive: boolean
}

export interface IPosition {
    positionID: number,
    title: string,
    description: string,
    accessLevel: number
}

export interface IHistory {
    historyID: number,
    changeDate: string,
    status: IRequestStatus,
    comment: string,
    changedBy: IEmployee
}

export interface IRequestType {
    requestTypeID: number,
    name: string,
    processingOrder: number
}