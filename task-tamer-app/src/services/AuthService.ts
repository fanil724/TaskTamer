import $api from "../http";
import {AuthResponce} from "../models/responce/AuthResponce";

export default class AuthService {
    static async login(username: string, password: string) {
        return $api.post<AuthResponce>(`/auth/login`, {username, password});
    }

    static async register(email: string, password: string) {
        return $api.post<AuthResponce>(`/auth/register`, {email, password});
    }

    static async logout(userID: number) {
        return $api.post(`/auth/logout/${userID}`);
    }

    static async checkAuth() {
        return $api.get<AuthResponce>(`/auth/profile`);
    }
}