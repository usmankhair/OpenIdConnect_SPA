import React from 'react';
import { useAuth } from '../context/AuthContext';

export const TokenDetails = () => {
    const { authToken } = useAuth();
    return (
        <div>
            <h1 id="" >User Details</h1>
            <p></p>

            <p><b>User Name:</b> {authToken.name}   </p>
            <p><b>Given Name:</b> {authToken.givenName}   </p>
            <p><b>Family Name:</b> {authToken.familyName}   </p>

        </div>
    );

}
