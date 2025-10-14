import $api from "../http";
import { IUser } from "../models/IUser";

export default class UserService {
    static getUserById(userid: number) {
        return $api.get<IUser>(`/user/${userid}`, { withCredentials: true });
    }
    static updateUser(user: IUser) {
        return $api.put<IUser>(`/user`, user, { withCredentials: true });
    }
   
}