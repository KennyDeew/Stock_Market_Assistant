import type { JwtPayload } from '../types/authTypes';

/**
 * Декодирует JWT-токен и возвращает payload
 * @param token - JWT в виде строки
 * @returns Объект с данными из payload или null, если ошибка
 */
export function parseJwt(token: string): JwtPayload | null {
    try {
        const base64Url = token.split('.')[1];
        if (!base64Url) {
            console.error('Invalid JWT: missing payload');
            return null;
        }

        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split('')
                .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
        );

        const payload = JSON.parse(jsonPayload);

        // Проверяем, что это объект
        if (typeof payload !== 'object' || payload === null) {
            return null;
        }

        return payload as JwtPayload;
    } catch (e) {
        console.error('Failed to parse JWT', e);
        return null;
    }
}
