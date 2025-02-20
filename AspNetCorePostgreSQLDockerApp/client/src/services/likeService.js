import api from './api';

// Provera statusa lajka
export const isLiked = async (userId, imageId, token) => {
    try {
        const response = await api.get(`/LikeService/isLiked?userId=${userId}&imageId=${imageId}`, {
            headers: { Authorization: `Bearer ${token}` }
        });
        return response.data;
    } catch (error) {
        console.error('Error checking like status:', error.response?.data || error.message || error);
        throw new Error('Error checking like status.');
    }
};

// Lajkovanje slike
export const likeImage = async (userId, imageId, token) => {
    if (!userId || !imageId) {
        throw new Error("Missing userId or imageId");
    }

    try {
        const response = await api.post(
            `/LikeService/like?userId=${userId}&imageId=${imageId}`,
            {},
            { headers: { Authorization: `Bearer ${token}` } }
        );
        return response.data;
    } catch (error) {
        console.error('Error liking image:', error.response?.data || error.message || error);
        throw new Error('Error liking image.');
    }
};


// Ukloni lajk sa slike
export const unlikeImage = async (userId, imageId, token) => {
    if (!userId || !imageId) {
        throw new Error("UserId or ImageId is missing");
    }

    try {
        const response = await api.delete(
            `/LikeService/unlike?userId=${userId}&imageId=${imageId}`,
            { headers: { Authorization: `Bearer ${token}` } }
        );
        return response.data; // Može biti potvrda o uklanjanju lajka
    } catch (error) {
        console.error('Error unliking image:', error.response?.data || error.message);
        throw new Error('Error unliking image.');
    }
};


// Vracanje broja lajkova
export const getLikeCount = async (imageId, token) => {
    try {
        const response = await api.get(`/LikeService/count?imageId=${imageId}`, {
            headers: { Authorization: `Bearer ${token}` }
        });
        return response.data.likeCount;
    } catch (error) {
        console.error('Error fetching like count:', error);
        return 0;
    }
};
