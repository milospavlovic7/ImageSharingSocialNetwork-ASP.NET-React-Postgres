import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { getUserById } from '../services/userService';
import { getUserImages } from '../services/imageService';
import { jwtDecode } from 'jwt-decode';
import ImageModal from './ImageModal';

function PublicProfile() {
    const { userId } = useParams();
    console.log("User ID from URL:", userId);

    const [user, setUser] = useState(null);
    const [images, setImages] = useState([]);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);
    const [selectedImage, setSelectedImage] = useState(null);

    const token = localStorage.getItem('jwtToken');
    const currentUserId = token
        ? jwtDecode(token)['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
        : null;

    useEffect(() => {
        if (!userId) {
            setError("Invalid user ID.");
            setLoading(false);
            return;
        }

        const fetchUserData = async () => {
            try {
                setLoading(true);
                const [userData, userImages] = await Promise.all([
                    getUserById(userId, token),
                    getUserImages(userId, token)
                ]);

                setUser(userData);
                setImages(userImages);
            } catch (err) {
                console.error("Error fetching user data:", err);
                setError("There was an error fetching data.");
            } finally {
                setLoading(false);
            }
        };

        fetchUserData();
    }, [userId, token]);

    if (loading) {
        return <p className="text-center">Loading...</p>;
    }

    if (error) {
        return <p className="text-center text-red-500">{error}</p>;
    }

    return (
        <div className="max-w-3xl mx-auto p-4 space-y-6">
            <h1 className="text-2xl font-bold mb-6 text-center">{user.name}</h1>
            {user ? (
                <div className="space-y-6">
                    {/* Korisnički detalji */}
                    <div className="flex items-center justify-between bg-white p-4 rounded-lg shadow-md">
                        <div>
                            <p><strong>Email:</strong> {user.email}</p>
                            <p><strong>Bio:</strong> {user.bio || 'No bio available'}</p>
                        </div>
                        {user.profilePicture && (
                            <div>
                                <img src={user.profilePicture} alt="Profile" className="w-24 h-24 rounded-full" />
                            </div>
                        )}
                    </div>

                    {/* Prikaz korisnikovih slika */}
                    <div className="bg-white p-4 rounded-lg shadow-md">
                        <h2 className="text-xl font-semibold">User Images</h2>
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
                </div>
            ) : (
                <p className="text-center">No user found.</p>
            )}

            {/* Render ImageModal kada je slika kliknuta */}
            {selectedImage && (
                <ImageModal
                    image={selectedImage}
                    onClose={() => setSelectedImage(null)}
                    token={token}
                    currentUserId={currentUserId}
                    allowEdit={currentUserId === userId} // Dozvola za edit samo ako je gledan sopstveni profil
                />
            )}
        </div>
    );
}

export default PublicProfile;
