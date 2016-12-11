import { Injectable } from '@angular/core';
import { Response } from '@angular/http';

import { AuthHttp } from 'angular2-jwt';

import { User }  from '../models/user';

@Injectable()
export class UserService {
    constructor(private authHttp: AuthHttp) { }

    getAll() {
        return this.authHttp.get('/api/users').map((response: Response) => response.json());
    }

    getById(id: number) {
        return this.authHttp.get('/api/users/' + id).map((response: Response) => response.json());
    }

    create(user: User) {
        return this.authHttp.post('/api/users', user).map((response: Response) => response.json());
    }

    //update(user: User) {
    //    return this.http.put('/api/users/' + user.id, user, this.jwt()).map((response: Response) => response.json());
    //}

    //delete(id: number) {
    //    return this.http.delete('/api/users/' + id, this.jwt()).map((response: Response) => response.json());
    //}

    // private helper methods
}