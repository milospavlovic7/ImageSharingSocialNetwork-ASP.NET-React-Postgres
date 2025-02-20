import axios from 'axios';

const apiImage = axios.create({
    baseURL: 'http://localhost:5000/api', // Promeni ako se API endpoint menja
});

export default apiImage;
