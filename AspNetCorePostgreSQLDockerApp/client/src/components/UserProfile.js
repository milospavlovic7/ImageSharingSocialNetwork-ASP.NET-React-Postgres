import React, { useEffect, useState } from 'react';
import { getUserById, updateUserProfile, changePassword } from '../services/userService';
import { getUserImages, addImage } from '../services/imageService';
import { jwtDecode } from 'jwt-decode';
import ImageModal from './ImageModal';

function UserProfile() {
    const [user, setUser] = useState(null);
    const [images, setImages] = useState([]);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);

    // Za upload slike
    const [imageFile, setImageFile] = useState(null);
    const [imageDescription, setImageDescription] = useState('');
    const [setAsProfilePicture, setSetAsProfilePicture] = useState(false);

    const token = localStorage.getItem('jwtToken');

    // Za uređivanje profila
    const [editName, setEditName] = useState('');
    const [editEmail, setEditEmail] = useState('');
    const [editBio, setEditBio] = useState('');
    const [updateMessage, setUpdateMessage] = useState('');

    // Za promenu lozinke
    const [oldPassword, setOldPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [passwordMessage, setPasswordMessage] = useState('');

    // Stanja za "collapsible" sekcije
    const [showEditProfile, setShowEditProfile] = useState(false);
    const [showChangePassword, setShowChangePassword] = useState(false);
    const [showUploadImage, setShowUploadImage] = useState(false);

    // Novo stanje za izabranu sliku (modal)
    const [selectedImage, setSelectedImage] = useState(null);

    useEffect(() => {
        if (!token) {
            setError('User is not logged in.');
            setLoading(false);
            return;
        }

        try {
            const decodedToken = jwtDecode(token);
            const userId = decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];

            if (!userId) {
                setError('Invalid token format.');
                setLoading(false);
                return;
            }

            // Učitavamo korisničke podatke i slike paralelno
            Promise.all([getUserById(userId, token), getUserImages(userId, token)])
                .then(([userData, userImages]) => {
                    setUser(userData);
                    setImages(userImages);
                    // Postavljamo inicijalne vrednosti za edit forme
                    setEditName(userData.name);
                    setEditEmail(userData.email);
                    setEditBio(userData.bio || '');
                    setLoading(false);
                })
                .catch((err) => {
                    setError('There was an error fetching data.');
                    console.error('Error fetching user data:', err);
                    setLoading(false);
                });
        } catch (err) {
            setError('Failed to decode the token.');
            setLoading(false);
            console.error('Token decoding error:', err.message);
        }
    }, [token]);

    // Funkcija za osvežavanje slika nakon brisanja
    const refreshImages = async () => {
        if (!user || !user.userId) return;
        try {
            const updatedImages = await getUserImages(user.userId, token);
            setImages(updatedImages);
        } catch (err) {
            console.error('Error refreshing images:', err);
            setError('Error refreshing images.');
        }
    };

    // Handler za upload slike
    const handleImageUpload = async (event) => {
        event.preventDefault();

        if (!imageFile) {
            setError('Please select an image.');
            return;
        }

        const formData = new FormData();
        formData.append('userId', user.userId);
        formData.append('description', imageDescription || 'Nova slika');
        formData.append('imageFile', imageFile, imageFile.name);

        try {
            const uploadedImage = await addImage(formData, token);
            setError(null);
            setImageFile(null);
            setImageDescription('');

            // Osvežavamo slike nakon upload-a
            const decodedToken = jwtDecode(token);
            const userId = decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
            const updatedImages = await getUserImages(userId, token);
            setImages(updatedImages);

            // Ako je čekirano, postavljamo upload-ovanu sliku kao profilnu sliku
            if (setAsProfilePicture && uploadedImage.imageUrl) {
                const updatedProfileData = {
                    Name: user.name,
                    Email: user.email,
                    Bio: user.bio,
                    ProfilePicture: uploadedImage.imageUrl,
                };
                await updateUserProfile(user.userId, updatedProfileData, token);
                // Osvežavamo korisničke podatke
                const updatedUser = await getUserById(user.userId, token);
                setUser(updatedUser);
                setSetAsProfilePicture(false);
            }
        } catch (err) {
            setError('There was an error uploading the image.');
            console.error('Error uploading image:', err);
        }
    };

    // Handler za update profila
    const handleProfileUpdate = async (event) => {
        event.preventDefault();
        try {
            const updatedProfileData = {
                Name: editName,
                Email: editEmail,
                Bio: editBio,
                ProfilePicture: user.profilePicture,
            };

            if (newPassword.trim()) {
                updatedProfileData.Password = newPassword;
            }

            await updateUserProfile(user.userId, updatedProfileData, token);
            setUpdateMessage('Profile updated successfully.');

            const updatedUser = await getUserById(user.userId, token);
            setUser(updatedUser);
        } catch (err) {
            setUpdateMessage('Error updating profile.');
            console.error('Profile update error:', err);
        }
    };

    // Handler za promenu lozinke
    const handlePasswordChange = async (event) => {
        event.preventDefault();
        setPasswordMessage('');

        if (!oldPassword || !newPassword) {
            setPasswordMessage('Please fill out both fields.');
            return;
        }

        try {
            await changePassword(user.userId, oldPassword, newPassword, token);
            setPasswordMessage('Password changed successfully.');
            setOldPassword('');
            setNewPassword('');
        } catch (err) {
            setPasswordMessage('Error changing password. Please check your old password.');
            console.error('Password change error:', err);
        }
    };

    return (
        <div className="max-w-3xl mx-auto p-4 space-y-6">
            <h1 className="text-2xl font-bold mb-6 text-center">User Profile</h1>
            {error && <p style={{ color: 'red' }}>{error}</p>}

            {loading ? (
                <p>Loading...</p>
            ) : (
                user && (
                    <div className="space-y-6">
                        {/* Korisnički detalji */}
                        <div className="flex items-center justify-between bg-white p-4 rounded-lg shadow-md">
                            <div>
                                <p>
                                    <strong>Name:</strong> {user.name}
                                </p>
                                <p>
                                    <strong>Email:</strong> {user.email}
                                </p>
                                <p>
                                    <strong>Bio:</strong> {user.bio || 'No bio available'}
                                </p>
                            </div>
                            {user.profilePicture && (
                                <div>
                                    <img
                                        src={user.profilePicture}
                                        alt="Profile"
                                        className="w-24 h-24 rounded-full"
                                    />
                                </div>
                            )}
                        </div>

                        {/* Prikaz korisničkih slika */}
                        <div className="bg-white p-4 rounded-lg shadow-md">
                            <h2 className="text-xl font-semibold">Your Images</h2>
                            {images && images.length > 0 ? (
                                <div className="grid grid-cols-3 gap-4">
                                    {images.map((img) => (
                                        <div
                                            key={img.imageId}
                                            className="rounded-lg overflow-hidden shadow-lg cursor-pointer transform hover:scale-105 transition"
                                            onClick={() => setSelectedImage(img)}
                                        >
                                            <img
                                                src={img.imageUrl}
                                                alt="User's Image"
                                                className="w-full h-48 object-cover"
                                            />
                                            <p className="text-center mt-2 text-sm">
                                                {img.description || 'No description'}
                                            </p>
                                        </div>
                                    ))}
                                </div>
                            ) : (
                                <p>No images to display</p>
                            )}
                        </div>

                        {/* Skriveni (collapsible) paneli */}
                        <div className="space-y-4">
                            {/* Upload Image panel */}
                            <div className="bg-gray-100 p-2 rounded-lg">
                                <button
                                    onClick={() => setShowUploadImage(!showUploadImage)}
                                    className="w-full text-left font-semibold"
                                >
                                    {showUploadImage ? 'Hide Upload Image' : 'Upload Image'}
                                </button>
                                {showUploadImage && (
                                    <div className="mt-4 bg-white p-4 rounded-lg shadow-md">
                                        <h2 className="text-xl font-semibold mb-4">Upload New Image</h2>
                                        <form onSubmit={handleImageUpload} className="space-y-4">
                                            <input
                                                type="file"
                                                onChange={(e) => setImageFile(e.target.files[0])}
                                                required
                                                className="w-full p-2 border rounded-lg"
                                            />
                                            <input
                                                type="text"
                                                placeholder="Image description"
                                                value={imageDescription}
                                                onChange={(e) => setImageDescription(e.target.value)}
                                                className="w-full p-2 border rounded-lg"
                                            />
                                            <div className="flex items-center">
                                                <input
                                                    type="checkbox"
                                                    id="setAsProfilePicture"
                                                    checked={setAsProfilePicture}
                                                    onChange={(e) => setSetAsProfilePicture(e.target.checked)}
                                                    className="mr-2"
                                                />
                                                <label htmlFor="setAsProfilePicture">Set as profile picture</label>
                                            </div>
                                            <button
                                                type="submit"
                                                className="w-full bg-blue-500 text-white py-2 rounded-lg hover:bg-blue-600 transition"
                                            >
                                                Upload Image
                                            </button>
                                        </form>
                                    </div>
                                )}
                            </div>

                            {/* Edit Profile panel */}
                            <div className="bg-gray-100 p-2 rounded-lg">
                                <button
                                    onClick={() => setShowEditProfile(!showEditProfile)}
                                    className="w-full text-left font-semibold"
                                >
                                    {showEditProfile ? 'Hide Edit Profile' : 'Edit Profile'}
                                </button>
                                {showEditProfile && (
                                    <div className="mt-4 bg-white p-4 rounded-lg shadow-md">
                                        <h2 className="text-xl font-semibold mb-4">Edit Profile</h2>
                                        {updateMessage && <p className="mb-2">{updateMessage}</p>}
                                        <form onSubmit={handleProfileUpdate} className="space-y-4">
                                            <div>
                                                <label className="block mb-1">Name:</label>
                                                <input
                                                    type="text"
                                                    value={editName}
                                                    onChange={(e) => setEditName(e.target.value)}
                                                    className="w-full p-2 border rounded-lg"
                                                />
                                            </div>
                                            <div>
                                                <label className="block mb-1">Email:</label>
                                                <input
                                                    type="email"
                                                    value={editEmail}
                                                    onChange={(e) => setEditEmail(e.target.value)}
                                                    className="w-full p-2 border rounded-lg"
                                                />
                                            </div>
                                            <div>
                                                <label className="block mb-1">Bio:</label>
                                                <textarea
                                                    value={editBio}
                                                    onChange={(e) => setEditBio(e.target.value)}
                                                    className="w-full p-2 border rounded-lg"
                                                />
                                            </div>
                                            <button
                                                type="submit"
                                                className="w-full bg-green-500 text-white py-2 rounded-lg hover:bg-green-600 transition"
                                            >
                                                Update Profile
                                            </button>
                                        </form>
                                    </div>
                                )}
                            </div>

                            {/* Change Password panel */}
                            <div className="bg-gray-100 p-2 rounded-lg">
                                <button
                                    onClick={() => setShowChangePassword(!showChangePassword)}
                                    className="w-full text-left font-semibold"
                                >
                                    {showChangePassword ? 'Hide Change Password' : 'Change Password'}
                                </button>
                                {showChangePassword && (
                                    <div className="mt-4 bg-white p-4 rounded-lg shadow-md">
                                        <h2 className="text-xl font-semibold mb-4">Change Password</h2>
                                        {passwordMessage && <p className="mb-2">{passwordMessage}</p>}
                                        <form onSubmit={handlePasswordChange} className="space-y-4">
                                            <div>
                                                <label className="block mb-1">Old Password:</label>
                                                <input
                                                    type="password"
                                                    value={oldPassword}
                                                    onChange={(e) => setOldPassword(e.target.value)}
                                                    className="w-full p-2 border rounded-lg"
                                                />
                                            </div>
                                            <div>
                                                <label className="block mb-1">New Password:</label>
                                                <input
                                                    type="password"
                                                    value={newPassword}
                                                    onChange={(e) => setNewPassword(e.target.value)}
                                                    className="w-full p-2 border rounded-lg"
                                                />
                                            </div>
                                            <button
                                                type="submit"
                                                className="w-full bg-purple-500 text-white py-2 rounded-lg hover:bg-purple-600 transition"
                                            >
                                                Change Password
                                            </button>
                                        </form>
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>
                )
            )}

            {/* Render ImageModal kada je slika kliknuta */}
            {selectedImage && (
                <ImageModal
                    image={selectedImage}
                    onClose={() => setSelectedImage(null)}
                    token={token}
                    currentUserId={user.userId}
                    allowEdit={true}  // U UserProfile slike su vaše, pa se omogućava edit i delete
                    onImageDeleted={refreshImages}  // Ovaj callback će osvežiti slike nakon brisanja
                />
            )}
        </div>
    );
}

export default UserProfile;
