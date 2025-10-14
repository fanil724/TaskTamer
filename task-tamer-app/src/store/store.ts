import { action, makeObservable, observable } from "mobx";
import AuthService from "../services/AuthService";

export class Store {
    userid = 0;
    username = "";
    role = "";
    department = '';
    employeeId = 0;
    userType = 'user';
    isAuth = false;
    isLoading = false;
    error = "";

    constructor() {
        makeObservable(this, {
            userid: observable,
            username: observable,
            role: observable,
            department: observable,
            employeeId: observable,
            userType: observable,
            isAuth: observable,
            isLoading: observable,
            error: observable,
            setAuth: action,
            setUser: action,
            setError: action,
            login: action,
            logout: action,
            checkAuth: action
        });
    }

    setAuth(bool: boolean) {
        this.isAuth = bool;
    }

    setUser(id: number, name: string, role: string, department: string, employeeId: number, usertype: string) {
        this.username = name;
        this.userid = id;
        this.role = role;
        this.department = department;
        this.employeeId = employeeId;        
        this.userType = usertype;
    }

    setError(message: string) {
        this.error = message;
    }

    async login(login: string, password: string) {
        try {
            this.isLoading = true;
            this.error = "";
            const response = await AuthService.login(login, password);
            
            if (response.data && response.data.userId) {
                this.setAuth(true);
                this.setUser(response.data.userId, response.data.username, response.data.role, response.data.department, response.data.employeeId, response.data.userType);
                return true;
            } else {
                throw new Error("Неверный ответ от сервера");
            }

        } catch (e: any) {
            console.error('Login error:', e);

            let errorMessage = "Ошибка авторизации";

            if (e.response?.data?.message) {
                errorMessage = e.response.data.message;
            } else if (e.message) {
                errorMessage = e.message;
            } else if (e.response?.status === 401) {
                errorMessage = "Неверный логин или пароль";
            } else if (e.response?.status === 400) {
                errorMessage = "Некорректные данные";
            } else if (e.response?.status >= 500) {
                errorMessage = "Ошибка сервера. Попробуйте позже";
            }

            this.setError(errorMessage);
            this.setAuth(false);
            this.setUser(0, "", "", "", 0, "");
            throw new Error(errorMessage);

        } finally {
            this.isLoading = false;
        }
    }

    async logout() {
        try {
            this.isLoading = true;
            this.error = "";

            const response = await AuthService.logout(this.userid);
            this.setAuth(false);
            this.setUser(0, "", "", "", 0, "");
            return true;

        } catch (e: any) {
            console.error('Logout error:', e);
            this.setAuth(false);
            this.setUser(0, "", "", "", 0, "");

            if (e.response?.data?.message) {
                console.log(e.response.data.message);
            }

            return false;
        } finally {
            this.isLoading = false;
        }
    }

    async checkAuth() {
        try {
            this.isLoading = true;
            this.error = "";

            const response = await AuthService.checkAuth();

            if (response.status === 200 && response.data) {
                this.setAuth(true);
                this.setUser(response.data.userId, response.data.username, response.data.role, response.data.department, response.data.employeeId, response.data.userType);
                return true;
            } else {
                this.setAuth(false);
                this.setUser(0, '', '', '', 0, "");
                return false;
            }

        } catch (e: any) {
            console.error('Auth check error:', e);

            this.setAuth(false);
            this.setUser(0, '', '', '', 0, "");

            if (e.response?.status === 401) {
                return false;
            }
            let errorMessage = "Ошибка проверки авторизации";
            if (e.response?.data?.message) {
                errorMessage = e.response.data.message;
            }
            this.setError(errorMessage);

            return false;

        } finally {
            this.isLoading = false;
        }
    }
}