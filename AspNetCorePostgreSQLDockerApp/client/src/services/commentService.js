import api from './api';

// Preuzmi sve komentare za određenu sliku
export const getCommentsForImage = async (imageId, token) => {
    try {
        const response = await api.get(`/CommentService?imageId=${imageId}`, {
            headers: { Authorization: `Bearer ${token}` }
        });
        console.log(response.data);  // Dodajemo log za odgovore
        return response.data;
    } catch (error) {
        throw new Error('Greška prilikom preuzimanja komentara.');
    }
};

// Dodaj komentar na sliku
export const postComment = async (imageId, content, userId, token) => {  // Dodan token
    try {
        const response = await api.post('/CommentService', {
            imageId,
            content,
            userId,
        }, {
            headers: { Authorization: `Bearer ${token}` }  // Dodan token u header
        });
        return response.data; // Možete vratiti podatke o komentaru
    } catch (error) {
        throw new Error('Error adding comment.');
    }
};

// Brisanje komentara prema ID-u
export const deleteComment = async (commentId, token) => {  // Dodan token
    try {
        await api.delete(`/CommentService/${commentId}`, {
            headers: { Authorization: `Bearer ${token}` }  // Dodan token u header
        });
        return { message: 'Comment deleted successfully.' };
    } catch (error) {
        throw new Error('Error deleting comment.');
    }
};
