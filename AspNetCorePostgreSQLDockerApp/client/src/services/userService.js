import api from './api';

// Get user data by ID (includes token in header)
export const getUserById = async (userId, token) => {
    try {
        const response = await api.get(`/UserService/${userId}`, {
            headers: { Authorization: `Bearer ${token}` }
        });
        return response.data;
    } catch (error) {
        throw new Error('Error fetching user data.');
    }
};

// Update user profile
export const updateUserProfile = async (userId, profileData, token) => {
    try {
        const response = await api.put(`/UserService/${userId}`, profileData, {
            headers: { Authorization: `Bearer ${token}` }
        });
        return response.data;
    } catch (error) {
        throw new Error('Error updating user profile.');
    }
};

// Change user password
export const changePassword = async (userId, oldPassword, newPassword, token) => {
    try {
        const response = await api.put(
            `/UserService/${userId}/change-password`, { oldPassword, newPassword },{
                headers: { Authorization: `Bearer ${token}` },
        });
        return response.data;
    } catch (error) {
        throw new Error("Failed to change password.");
    }
};
