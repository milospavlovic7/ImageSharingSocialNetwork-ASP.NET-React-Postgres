import api from './api';

// Login funkcija koja šalje zahtev za autentifikaciju
export const loginUser = async (email, password) => {
    try {
        const response = await api.post('/UserService/login', { email, password });
        const token = response.data.token;

        // Spremi JWT token u localStorage
        localStorage.setItem('jwtToken', token);

        return response.data; // Možete vratiti podatke kao što su token, user data, itd.
    } catch (error) {
        throw new Error('Login failed. Please try again.');
    }
};

// Registracija korisnika
export const register = async (name, email, password) => {
    try {
        await api.post('/UserService/register', { name, email, password });
        return { message: 'Registration successful! Please login.' };
    } catch (error) {
        throw new Error('Error during registration. Please try again.');
    }
};

// Funkcija za logout
export const logout = () => {
    localStorage.removeItem('jwtToken');
    window.location.href = '/login'; // Redirektuj korisnika na login stranicu
};

// Funkcija za proveru da li je korisnik ulogovan
export const isAuthenticated = () => {
    return !!localStorage.getItem('jwtToken');
};
