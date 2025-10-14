import {IEmployee} from "./IEmployee";
import {IDepartment} from "./IDepartment";

export interface IEquipment {
    equipmentID:number,
    name:string,
    model:string,
    serialNumber:string,
    type:string,
    manufacturer:string,
    purchaseDate:Date,
    responsibleEmployee:IEmployee,
    departmentDTO:IDepartment,
    location:string,
    technicalDocumentation:string
}