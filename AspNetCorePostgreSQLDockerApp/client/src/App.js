import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Login from './components/Login';
import Register from './components/Register';
import UserProfile from './components/UserProfile';
import Feed from './components/Feed'; // Uvozimo Feed komponentu
import PublicProfile from './components/PublicProfile'; // Dodajemo PublicProfile komponentu
import Navbar from './components/Navbar'; // Uvozimo Navbar
import { isAuthenticated } from './services/authService'; // Uvozimo funkciju za autentifikaciju

function App() {
    // Komponenta koja se prikazuje na početnoj stranici
    const Home = () => (
        <div className="flex flex-col justify-center items-center bg-blue-500 min-h-screen text-white">
            <h1 className="text-4xl font-bold mb-8">Welcome to the App</h1>
            <button
                onClick={() => window.location.href = '/login'}
                className="bg-green-500 text-white py-2 px-4 rounded-lg hover:bg-green-700 mb-4"
            >
                Login
            </button>
            <button
                onClick={() => window.location.href = '/register'}
                className="bg-orange-500 text-white py-2 px-4 rounded-lg hover:bg-orange-700"
            >
                Register
            </button>
        </div>
    );

    // Provera da li je korisnik autentifikovan pomoću authService
    const isAuthenticatedUser = isAuthenticated();

    return (
        <Router>
            {/* Navbar koji se prikazuje na svim stranicama */}
            <Navbar />

            <Routes>
                {/* Početna stranica */}
                <Route path="/" element={<Home />} />

                {/* Rute za login i registraciju */}
                <Route path="/login" element={<Login />} />
                <Route path="/register" element={<Register />} />

                {/* Rute koje su zaštićene, moraš biti autentifikovan */}
                <Route path="/profile" element={isAuthenticatedUser ? <UserProfile /> : <Navigate to="/login" />} />
                <Route path="/feed" element={isAuthenticatedUser ? <Feed /> : <Navigate to="/login" />} />

                {/* Rute za tuđe profile */}
                <Route path="/profile/:userId" element={<PublicProfile />} /> {/* Ovaj parametar će biti dinamicki */}

                {/* Defaultna ruta (ako korisnik ne ide na postojeću stranicu) */}
                <Route path="*" element={<Navigate to="/" />} />
            </Routes>
        </Router>
    );
}

export default App;
