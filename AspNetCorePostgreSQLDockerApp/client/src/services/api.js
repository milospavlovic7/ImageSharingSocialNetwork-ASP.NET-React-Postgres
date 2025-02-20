import axios from 'axios';

const api = axios.create({
    baseURL: 'http://localhost:5000/api', // Promeni ako se API endpoint menja
    headers: {
        'Content-Type': 'application/json',
    },
});

export default api;

