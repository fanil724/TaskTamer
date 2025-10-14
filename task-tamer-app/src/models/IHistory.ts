import {IRequestStatus} from "./IRequestStatus";
import {IEmployee} from "./IEmployee";

export interface IHistory{
    historyID:number,
    changeDate:Date,
    status:IRequestStatus,
    comment:string,
    changedBy:IEmployee
}