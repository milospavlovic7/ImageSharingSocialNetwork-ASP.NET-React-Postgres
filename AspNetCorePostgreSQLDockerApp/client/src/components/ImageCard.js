import React, { useEffect, useState } from 'react';
import { getImage } from '../services/imageService';
import { isLiked, likeImage, unlikeImage, getLikeCount } from '../services/likeService';
import { getCommentsForImage, postComment } from '../services/commentService';
import { formatDistanceToNow } from 'date-fns';
import { jwtDecode } from 'jwt-decode';
import { useNavigate } from 'react-router-dom';

function ImageCard({ imageId }) {
    const [image, setImage] = useState(null);
    const [comments, setComments] = useState([]);
    const [visibleComments, setVisibleComments] = useState(3);
    const [newComment, setNewComment] = useState('');
    const [isImageLiked, setIsImageLiked] = useState(false);
    const [error, setError] = useState(null);
    const token = localStorage.getItem('jwtToken');
    const decodedToken = jwtDecode(token);
    const userId = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
    const [likeCount, setLikeCount] = useState(0);
    const navigate = useNavigate();


    useEffect(() => {
        if (!token) {
            setError('You need to be logged in to view this image.');
            return;
        }

        getImage(imageId, token)
            .then((data) => setImage(data))
            .catch(() => setError('Error fetching image data'));

        getCommentsForImage(imageId, token)
            .then((data) => setComments(data))
            .catch(() => setError('Error fetching comments'));

        isLiked(userId, imageId, token)
            .then((data) => setIsImageLiked(data.isLiked))
            .catch(() => console.error('Error checking like status'));

        getLikeCount(imageId, token)
            .then((count) => setLikeCount(count))
            .catch(() => console.error('Error fetching like count'));
    }, [imageId, token, userId]);

    const handleLike = () => {
        if (!userId) {
            setError("User not logged in.");
            return;
        }

        const likeAction = isImageLiked ? unlikeImage : likeImage;
        likeAction(userId, imageId, token)
            .then(() => {
                setIsImageLiked(!isImageLiked);
                setLikeCount((prev) => (isImageLiked ? prev - 1 : prev + 1)); // Ažuriraj broj lajkova
            })
            .catch(() => console.error('Error liking image'));
    };

    const handleCommentSubmit = async (event) => {
        event.preventDefault();
        try {
            const response = await postComment(imageId, newComment, userId, token);
            console.log('Server response:', response);

            if (response && response.commentId) {
                await getCommentsForImage(imageId, token)
                    .then((data) => setComments(data));
                setNewComment('');
            } else {
                console.error("Response nije u ispravnom formatu:", response);
            }
        } catch (error) {
            console.error('Greška prilikom slanja komentara:', error.message);
        }
    };

    const showMoreComments = () => {
        setVisibleComments((prev) => prev + 3);
    };

    const hideComments = () => {
        setVisibleComments(3);
    };

    const navigateLogic = () => {
        console.log('Image User data:', image.user);
        console.log('userID Data:', userId);
        if (image.user.userId === userId) { // upoređujemo sa ispravnim property-jem
            navigate(`/profile`);  // Ako je korisnik sam, vodi ga na njegov profil
        } else {
            navigate(`/profile/${image.user.userId}`);  // Inače vodi na tuđi profil
        }
    };


    return (
        <div className="max-w-3xl mx-auto bg-white rounded-lg shadow-lg p-6 mb-8">
            {error && <p className="text-red-500 text-center">{error}</p>}

            {image && (
                <div>
                    <div className="flex items-center justify-center space-x-3 mb-4 cursor-pointer" onClick={() => navigateLogic()}>
                        <img src={image.user.profilePicture} alt={image.user.name} className="w-10 h-10 rounded-full" />
                        <span className="font-semibold text-lg text-blue-600">{image.user.name}</span>
                    </div>

                    <div className="relative mb-4">
                        <img
                            src={image.imageUrl}
                            alt={image.description}
                            className="w-full h-auto max-h-96 object-contain rounded-lg"
                        />
                    </div>
                    <h3 className="text-2xl font-semibold mt-2 text-center">{image.description}</h3>
                    <p className="text-sm text-gray-500 text-center mt-1">
                        {formatDistanceToNow(new Date(image.createdAt), { addSuffix: true })}
                    </p>

                    <div className="flex justify-center items-center mt-4">
                        <button
                            onClick={handleLike}
                            className={`py-2 px-4 rounded-lg ${isImageLiked ? 'bg-blue-500' : 'bg-gray-300'} text-white`}
                        >
                            {isImageLiked ? 'Unlike' : 'Like'}
                        </button>
                        <span className="ml-2 text-gray-700">{likeCount} {likeCount === 1 ? 'Like' : 'Likes'}</span>
                    </div>

                    <div className="mt-6">
                        <h4 className="text-lg font-semibold mb-2">Comments:</h4>
                        <div>
                            {comments.length === 0 ? (
                                <p>No comments yet.</p>
                            ) : (
                                comments.slice(0, visibleComments).map((comment) => (
                                    <div key={comment.commentId} className="border-b py-2">
                                        <p className="text-sm">{comment.content}</p>
                                        <p className="text-xs text-gray-400">
                                            - {comment.user.name}, {formatDistanceToNow(new Date(comment.createdAt), { addSuffix: true })}
                                        </p>
                                    </div>
                                ))
                            )}
                        </div>

                        <div className="flex space-x-4 mt-2 justify-center">
                            {visibleComments < comments.length && (
                                <button
                                    onClick={showMoreComments}
                                    className="text-blue-500 hover:underline"
                                >
                                    Show more comments
                                </button>
                            )}
                            {visibleComments > 3 && (
                                <button
                                    onClick={hideComments}
                                    className="text-red-500 hover:underline"
                                >
                                    Hide comments
                                </button>
                            )}
                        </div>

                        <form onSubmit={handleCommentSubmit} className="mt-4">
                            <textarea
                                value={newComment}
                                onChange={(e) => setNewComment(e.target.value)}
                                placeholder="Add a comment..."
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
            )}
        </div>
    );
}

export default ImageCard;
