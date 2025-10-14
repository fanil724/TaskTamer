import {IPosition} from "./IPosition";
import {IDepartment} from "./IDepartment";
export interface IEmployee {
    employeeID:number,
    fullName:string,
    positionDTO:IPosition,
    departmentDTO:IDepartment,
    phone:string,
    email:string,
    userType:string,
    registrationDate:Date,
    terminationDate:Date|null,
    isActive:boolean
}