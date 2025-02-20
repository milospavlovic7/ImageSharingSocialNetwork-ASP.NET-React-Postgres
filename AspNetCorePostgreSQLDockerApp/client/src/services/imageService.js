import api from './api';
import apiImage from './apiImage';

// Preuzmi slike sa servera sa paginacijom
export const getImages = async (token, page = 1, pageSize = 10) => {
    try {
        const response = await api.get(`/ImageService?page=${page}&pageSize=${pageSize}`, {
            headers: { Authorization: `Bearer ${token}` },
        });
        return response.data;
    } catch (error) {
        throw new Error('Error fetching images.');
    }
};

// Preuzmi sliku prema ID-u
export const getImage = async (imageId, token) => {
    try {
        const response = await api.get(`/ImageService/${imageId}`, {
            headers: { Authorization: `Bearer ${token}` }
        });
        return response.data;
    } catch (error) {
        throw new Error('Error fetching image data.');
    }
};

// Preuzmi slike korisnika prema korisničkom ID-u
export const getUserImages = async (userId, token) => {
    try {
        const response = await api.get(`/ImageService/user/${userId}`, {
            headers: { Authorization: `Bearer ${token}` }
        });
        return response.data;
    } catch (error) {
        console.error("Error fetching user images:", error.response?.data || error.message);

        if (error.response && error.response.status === 404 && error.response.data?.message === "No images found for this user.") {
            return []; // Vrati prazan niz umesto da baca grešku
        }

        throw new Error('Error fetching user images.');
    }
};

// Dodaj novu sliku
export const addImage = async (imageData, token) => {
    try {
        const response = await apiImage.post('/ImageService', imageData, {
            headers: { Authorization: `Bearer ${token}` }
        });
        return response.data;
    } catch (error) {
        if (error.response) {
            console.error("Error response data:", error.response.data);
            console.error("Error status:", error.response.status);
        } else {
            console.error("Error message:", error.message);
        }
        throw new Error('Error uploading image.');
    }
};

// Brisanje slike prema ID-u
export const deleteImage = async (imageId, token) => {  // Dodan token
    try {
        await api.delete(`/ImageService/${imageId}`, {
            headers: { Authorization: `Bearer ${token}` }  // Dodan token u header
        });
        return { message: 'Image deleted successfully.' };
    } catch (error) {
        throw new Error('Error deleting image.');
    }
};


export const updateImageDescription = async (imageId, description, token) => {
    try {
        await api.put(`/ImageService/${imageId}`,
            { description },
            { headers: { Authorization: `Bearer ${token}` } } // Token u headeru, bez JSON Content-Type
        );
        return { message: 'Image description updated successfully.' };
    } catch (error) {
        throw new Error('Error updating image description.');
    }
};

