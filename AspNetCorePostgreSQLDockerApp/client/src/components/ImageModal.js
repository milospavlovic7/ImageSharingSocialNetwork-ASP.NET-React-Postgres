import React, { useState, useEffect } from 'react';
import { deleteImage, updateImageDescription } from '../services/imageService';
import { isLiked, likeImage, unlikeImage, getLikeCount } from '../services/likeService';
import { getCommentsForImage, postComment } from '../services/commentService';
import { formatDistanceToNow } from 'date-fns';
import { useNavigate } from 'react-router-dom';

const ImageModal = ({ image, onClose, token, currentUserId, allowEdit, onImageDeleted }) => {
    // Like status
    const [likeCount, setLikeCount] = useState(0);
    const [liked, setLiked] = useState(false);

    // Lokalno stanje za opis slike
    const [currentDescription, setCurrentDescription] = useState(image.description);
    const [editDescription, setEditDescription] = useState(image.description);
    const [isEditingDescription, setIsEditingDescription] = useState(false);
    const [error, setError] = useState('');

    // Komentari
    const [comments, setComments] = useState([]);
    const [newComment, setNewComment] = useState('');

    const navigate = useNavigate();

    // Učitavanje like statusa i broja lajkova
    useEffect(() => {
        async function fetchLikeData() {
            try {
                const count = await getLikeCount(image.imageId, token);
                setLikeCount(count);
                const likedStatus = await isLiked(currentUserId, image.imageId, token);
                setLiked(likedStatus.isLiked);
            } catch (err) {
                console.error(err);
                setError('Error loading like data.');
            }
        }
        fetchLikeData();
    }, [image.imageId, token, currentUserId]);

    // Učitavanje komentara
    useEffect(() => {
        async function fetchComments() {
            try {
                const data = await getCommentsForImage(image.imageId, token);
                setComments(data);
            } catch (err) {
                console.error(err);
                setError('Error fetching comments.');
            }
        }
        fetchComments();
    }, [image.imageId, token]);

    // Handler za lajkovanje
    const handleLike = async () => {
        if (!currentUserId) {
            setError("User not logged in.");
            return;
        }
        try {
            if (liked) {
                await unlikeImage(currentUserId, image.imageId, token);
                setLiked(false);
                setLikeCount(prev => prev - 1);
            } else {
                await likeImage(currentUserId, image.imageId, token);
                setLiked(true);
                setLikeCount(prev => prev + 1);
            }
        } catch (err) {
            console.error(err);
            setError('Error updating like status.');
        }
    };

    // Handler za izmenu opisa slike
    const handleDescriptionUpdate = async () => {
        try {
            await updateImageDescription(image.imageId, editDescription, token);
            // Ažuriramo lokalno stanje opisa
            setCurrentDescription(editDescription);
            setIsEditingDescription(false);
        } catch (err) {
            console.error(err);
            setError('Error updating description.');
        }
    };

    // Handler za brisanje slike
    const handleDeleteImage = async () => {
        try {
            await deleteImage(image.imageId, token);
            // Poziv callback-a za osvežavanje slika u UserProfile
            if (onImageDeleted) {
                onImageDeleted();
            }
            onClose();
        } catch (err) {
            console.error(err);
            setError('Error deleting image.');
        }
    };

    // Handler za slanje komentara
    const handleCommentSubmit = async (e) => {
        e.preventDefault();
        if (!newComment.trim()) return;
        try {
            const response = await postComment(image.imageId, newComment, currentUserId, token);
            if (response && response.commentId) {
                const data = await getCommentsForImage(image.imageId, token);
                setComments(data);
                setNewComment('');
            } else {
                console.error("Response format error:", response);
            }
        } catch (err) {
            console.error(err);
            setError('Error posting comment.');
        }
    };

    // Navigacija na profil korisnika
    const navigateLogic = () => {
        if (image.user) {
            if (image.user.userId === currentUserId) {
                navigate('/profile');
            } else {
                navigate(`/profile/${image.user.userId}`);
            }
        }
        onClose();
    };

    return (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-60 z-50 overflow-auto">
            <div className="bg-white p-6 rounded-lg max-w-5xl w-full relative">
                <button
                    onClick={onClose}
                    className="absolute top-2 right-2 text-3xl font-bold text-gray-600"
                >
                    &times;
                </button>
                {error && <p className="text-red-500 mb-2">{error}</p>}
                <div className="flex flex-col md:flex-row">
                    {/* Leva strana – uvećana slika */}
                    <div className="flex-1 flex justify-center items-center mb-4 md:mb-0">
                        <img
                            src={image.imageUrl}
                            alt={currentDescription}
                            className="max-h-[500px] object-contain rounded-lg"
                        />
                    </div>
                    {/* Desna strana – detalji */}
                    <div className="flex-1 p-4">
                        {/* Korisnički podaci */}
                        {image.user && (
                            <div
                                className="flex items-center space-x-3 mb-4 cursor-pointer"
                                onClick={navigateLogic}
                            >
                                <img
                                    src={image.user.profilePicture}
                                    alt={image.user.name}
                                    className="w-10 h-10 rounded-full"
                                />
                                <span className="font-semibold text-lg text-blue-600">
                                    {image.user.name}
                                </span>
                            </div>
                        )}

                        {/* Opis slike – opcije za edit ako je dozvoljeno */}
                        <div className="mb-4">
                            {allowEdit && image.user && image.user.userId === currentUserId ? (
                                isEditingDescription ? (
                                    <>
                                        <input
                                            type="text"
                                            value={editDescription}
                                            onChange={(e) => setEditDescription(e.target.value)}
                                            className="w-full p-2 border rounded"
                                        />
                                        <div className="mt-2">
                                            <button
                                                onClick={handleDescriptionUpdate}
                                                className="bg-blue-500 text-white px-4 py-2 rounded"
                                            >
                                                Save
                                            </button>
                                            <button
                                                onClick={() => {
                                                    setIsEditingDescription(false);
                                                    setEditDescription(currentDescription);
                                                }}
                                                className="ml-2 bg-gray-500 text-white px-4 py-2 rounded"
                                            >
                                                Cancel
                                            </button>
                                        </div>
                                    </>
                                ) : (
                                    <div className="flex items-center justify-between">
                                        <h3 className="text-2xl font-semibold">
                                            {currentDescription || 'No description'}
                                        </h3>
                                        <div>
                                            <button
                                                onClick={() => setIsEditingDescription(true)}
                                                className="text-blue-500 text-sm mr-2"
                                            >
                                                Edit
                                            </button>
                                            <button
                                                onClick={handleDeleteImage}
                                                className="text-red-500 text-sm"
                                            >
                                                Delete
                                            </button>
                                        </div>
                                    </div>
                                )
                            ) : (
                                <h3 className="text-2xl font-semibold">
                                    {currentDescription || 'No description'}
                                </h3>
                            )}
                        </div>

                        {/* Like dugme i broj lajkova */}
                        <div className="flex items-center mb-4">
                            <button
                                onClick={handleLike}
                                className={`py-2 px-4 rounded-lg ${liked ? 'bg-blue-500' : 'bg-gray-300'} text-white`}
                            >
                                {liked ? 'Unlike' : 'Like'}
                            </button>
                            <span className="ml-2 text-gray-700">
                                {likeCount} {likeCount === 1 ? 'Like' : 'Likes'}
                            </span>
                        </div>

                        {/* Komentari */}
                        <div className="mt-6">
                            <h4 className="text-lg font-semibold mb-2">Comments:</h4>
                            <div className="h-60 overflow-y-auto">
                                {comments.length === 0 ? (
                                    <p>No comments yet.</p>
                                ) : (
                                    comments.map((comment) => (
                                        <div key={comment.commentId} className="border-b py-2">
                                            <p className="text-sm">{comment.content}</p>
                                            <p className="text-xs text-gray-400">
                                                - {comment.user.name}, {formatDistanceToNow(new Date(comment.createdAt), { addSuffix: true })}
                                            </p>
                                        </div>
                                    ))
                                )}
                            </div>

                            <form onSubmit={handleCommentSubmit} className="mt-4">
                                <textarea
                                    value={newComment}
                                    onChange={(e) => setNewComment(e.target.value)}
                                    placeholder="Add a comment..."
                                    rows="4"
                                    className="w-full p-2 border rounded-md"
                                />
                                <button
                                    type="submit"
                                    className="bg-blue-500 text-white py-2 px-4 rounded-lg mt-2 w-full"
                                >
                                    Post Comment
                                </button>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ImageModal;
