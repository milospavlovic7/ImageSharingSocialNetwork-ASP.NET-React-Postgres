import React, { useState, useEffect } from 'react';
import { getComments, addComment } from '../services/commentService'; // Uvozimo funkcije iz servisa

function CommentSection({ imageId }) {
    const [comments, setComments] = useState([]);
    const [newComment, setNewComment] = useState('');
    const [error, setError] = useState(null);

    useEffect(() => {
        if (imageId) {
            // Dohvatanje komentara za određenu sliku
            getComments(imageId)
                .then((data) => {
                    setComments(data); // Postavljanje komentara u stanje
                })
                .catch((error) => {
                    setError('Failed to fetch comments');
                    console.error(error);
                });
        }
    }, [imageId]);

    const handleCommentChange = (e) => {
        setNewComment(e.target.value); // Ažuriranje stanja komentara
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        if (!newComment.trim()) return; // Ako je komentar prazan, ne šaljemo

        // Dodavanje novog komentara putem servisa
        addComment(newComment, imageId)
            .then((newCommentData) => {
                setComments([...comments, newCommentData]); // Dodavanje novog komentara u listu
                setNewComment(''); // Brisanje input polja
            })
            .catch((error) => {
                setError('Failed to post comment');
                console.error(error);
            });
    };

    return (
        <div className="comment-section mt-4">
            <h3 className="font-semibold text-lg">Comments</h3>
            {error && <p className="text-red-500">{error}</p>}
            <div className="comments-list mt-2">
                {comments.length > 0 ? (
                    comments.map((comment) => (
                        <div key={comment.commentId} className="comment mb-2 p-2 border-b">
                            <p>{comment.content}</p>
                            <small>By {comment.user.name} on {new Date(comment.createdAt).toLocaleString()}</small>
                        </div>
                    ))
                ) : (
                    <p>No comments yet.</p>
                )}
            </div>

            <form onSubmit={handleSubmit} className="mt-4">
                <textarea
                    value={newComment}
                    onChange={handleCommentChange}
                    placeholder="Write a comment..."
                    rows="4"
                    className="w-full p-2 border rounded-md"
                />
                <button
                    type="submit"
                    className="mt-2 bg-blue-500 text-white px-4 py-2 rounded-md"
                >
                    Post Comment
                </button>
            </form>
        </div>
    );
}

export default CommentSection;
