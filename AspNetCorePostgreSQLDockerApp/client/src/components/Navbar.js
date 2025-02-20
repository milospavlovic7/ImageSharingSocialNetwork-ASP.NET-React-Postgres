import React from 'react';
import { Link, useNavigate } from 'react-router-dom';

function Navbar() {
    const navigate = useNavigate(); // Korišćenje useNavigate za preusmeravanje

    const handleLogout = () => {
        // Briše JWT token iz localStorage i vrati korisnika na login stranicu
        localStorage.removeItem('jwtToken');

        // Preusmeri korisnika na login stranicu koristeći useNavigate
        navigate('/login');
    };

    const token = localStorage.getItem('jwtToken');

    return (
        <nav className="bg-blue-500 p-4">
            <div className="max-w-7xl mx-auto flex justify-between items-center">
                <div className="text-white font-bold text-lg">
                    <Link to={token ? "/feed" : "/"}>Social Network</Link> {/* Prilagodba linka */}
                </div>
                <div>
                    <ul className="flex space-x-4">
                        {token && (
                            <li>
                                <Link to="/feed" className="text-white hover:text-gray-300">
                                    Feed
                                </Link>
                            </li>
                        )}
                        {token ? (
                            <>
                                <li>
                                    <Link to="/profile" className="text-white hover:text-gray-300">
                                        Profile
                                    </Link>
                                </li>
                                <li>
                                    <button
                                        onClick={handleLogout}
                                        className="text-white hover:text-gray-300"
                                    >
                                        Logout
                                    </button>
                                </li>
                            </>
                        ) : (
                            <>
                                <li>
                                    <Link to="/login" className="text-white hover:text-gray-300">
                                        Login
                                    </Link>
                                </li>
                                <li>
                                    <Link to="/register" className="text-white hover:text-gray-300">
                                        Register
                                    </Link>
                                </li>
                            </>
                        )}
                    </ul>
                </div>
            </div>
        </nav>
    );
}

export default Navbar;
