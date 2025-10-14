import { IEmployee } from "./IRequest";

export interface IUser {
    userID: number,
    employeeDTO: IEmployee,
    username: string,
    passwordHash: string,
    registrationDate: Date,
    isActive: boolean,
    roleDTO: IRole
}

export interface IRole {
    roleID: number,
    name: string,
    description: string,
    accessLevel: number
}