import React, { useEffect, useState } from 'react';
import { getImages } from '../services/imageService'; // Uvozimo getImages iz imageService
import ImageCard from './ImageCard';

function Feed() {
    const [images, setImages] = useState([]); // Držimo sve slike koje su učitane
    const [error, setError] = useState(null);
    const [page, setPage] = useState(1); // Trenutna strana
    const [loading, setLoading] = useState(false);
    const [hasMore, setHasMore] = useState(true); // Proveri ima li još slika
    const token = localStorage.getItem('jwtToken');

    useEffect(() => {
        if (!token) {
            setError('You need to be logged in to view the feed.');
            return;
        }

        // Preuzmi slike sa servera
        const loadImages = async () => {
            setLoading(true);
            try {
                const data = await getImages(token, page);
                console.log('Data from server:', data);  // Dodaj ovo za logovanje odgovora sa servera

                if (Array.isArray(data)) {
                    if (data.length === 0) {
                        setHasMore(false); // Nema više slika
                    } else {
                        setImages((prevImages) => {
                            const newImages = data.filter(image =>
                                !prevImages.some(prev => prev.imageId === image.imageId)
                            );
                            return [...prevImages, ...newImages];
                        });
                    }
                } else {
                    throw new Error('Data is not an array');
                }
            } catch (err) {
                setError('Error fetching images');
                console.error('Error fetching images:', err);
            } finally {
                setLoading(false); // Postavi loading na false uvek
            }
        };



        loadImages();
    }, [token, page]);

    const loadMoreImages = () => {
        if (loading) return; // Ako je već u toku učitavanje, ne dozvoljavaj novo učitavanje
        setPage((prevPage) => prevPage + 1); // Povećaj broj strane za sledeći load
    };

    return (
        <div className="p-4">
            {error && <p className="text-red-500">{error}</p>}
            {!images.length && !error ? (
                <p>Loading images...</p>
            ) : (
                images.map((image) => (
                    <ImageCard key={image.imageId} imageId={image.imageId} />
                ))
            )}
            {loading && <p>Loading more...</p>}
            <div className="text-center mt-4">
                {/* Dugme "Load more" se prikazuje samo kada je učitavanje završeno i ima još slika */}
                {!loading && hasMore && images.length > 0 && (
                    <button
                        onClick={loadMoreImages}
                        className="bg-blue-500 text-white py-2 px-4 rounded-lg"
                    >
                        Load more
                    </button>
                )}
                {/* Poruka kad nema više slika */}
                {!hasMore && images.length > 0 && !loading && (
                    <p>No more images to load</p>
                )}
            </div>
        </div>
    );
}

export default Feed;
