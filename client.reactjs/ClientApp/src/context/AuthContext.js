import React, { useState, useEffect, useContext } from "react";
import { Redirect, Route } from 'react-router';
export const AuthContext = React.createContext();
export const useAuth = () => useContext(AuthContext);
export const AuthProvider = ({
    children
}) => {
    const [isAuthenticated, setIsAuthenticated] = useState();
    const [user, setUser] = useState();
    const [authToken, setAuthToken] = useState();
    const [isLoading, setIsLoading] = useState(false);

    const getUser = async () => {
        const response = await fetch('/auth/external/getUser');
        const json = await response.json();
        setIsAuthenticated(true);
        setAuthToken(json);
    }

    const getAuthentication = async () => {
        try {
            console.log("calling getAuthentication");
            // this will be changed to POST with all parameters to the server
            var code = findGetParameter("code");   //for short cut :-D 
            const response = await fetch('/auth/external/authenticate?code=' + code);
            const json = await response.json();
            setIsAuthenticated(true);
            setAuthToken(json);

            // redirect to root
            // return <Redirect to="/" />

            // TODO : Redirect with router
        }
        catch (ex) {
            ;
        }

    }



    useEffect(() => {

        //getUser();
    }, []);

    const login = () => {

        window.location.href = '/auth/external/login';
    }

    const logout = () => {
        window.location.href = '/auth/external/logout';
    }

    return (
        <AuthContext.Provider
            value={{
                isAuthenticated,
                authToken,
                user,
                isLoading,
                login,
                logout,
                getAuthentication
            }}
        >
            {children}
        </AuthContext.Provider>
    );
};

function getCookie(cname) {
    let name = cname + "=";
    let ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

function findGetParameter(parameterName) {
    var result = null,
        tmp = [];
    window.location.search
        .substr(1)
        .split("&")
        .forEach(function (item) {
            tmp = item.split("=");
            if (tmp[0] === parameterName) result = decodeURIComponent(tmp[1]);
        });
    return result;
}
