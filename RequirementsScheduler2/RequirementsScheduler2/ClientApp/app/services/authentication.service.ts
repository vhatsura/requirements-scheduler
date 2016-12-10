import { Injectable, OnInit } from '@angular/core';
import { Http, Headers, Response, RequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { isBrowser } from 'angular2-universal';
import 'rxjs/add/operator/map';

import { User } from '../models/user';

@Injectable()
export class AuthenticationService implements OnInit {

    // Observable navItem source
    private _user = new BehaviorSubject<User>(null);

    user = this._user.asObservable();

    constructor(private http: Http) { }

    login(username: string, password: string): Observable<boolean> {
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers: headers });
        return this.http.post('/api/users/authenticate', JSON.stringify({ username: username, password: password }), options)
            .map((response: Response) => {
                // login successful if there's a jwt token in the response
                let user = response.json();
                if (user && user.token) {
                    if (isBrowser) {
                        // store user details and jwt token in local storage to keep user logged in between page refreshes
                        localStorage.setItem('currentUser', JSON.stringify(user));
                        this._user.next(user);

                        return true;
                    }

                }

                return false;
            });
    }

    logout() {
        if (isBrowser) {
            // remove user from local storage to log user out
            localStorage.removeItem('currentUser');
            this._user.next(null);
        }
    }

    ngOnInit(): void {
        if (isBrowser) {
            if (localStorage.getItem('currentUser')) {
                let user = JSON.parse(localStorage.getItem('currentUser'));
                this._user.next(user);
            }
        }
    }
}