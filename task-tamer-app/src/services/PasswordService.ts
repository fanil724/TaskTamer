
import $api from '../http';

export interface ChangePasswordData {
    OLDPassword: string;
    NewPassword: string;   
}

export default class PasswordService {
    static async changePassword(data: ChangePasswordData) {
        return $api.post('/auth/change-password', data);
    }

    static async validatePassword(password: string): Promise<boolean> {

        const minLength = 8;
        const hasUpperCase = /[A-Z]/.test(password);
        const hasLowerCase = /[a-z]/.test(password);
        const hasNumbers = /\d/.test(password);
        const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(password);

        return password.length >= minLength &&
            hasUpperCase &&
            hasLowerCase &&
            hasNumbers &&
            hasSpecialChar;
    }
}