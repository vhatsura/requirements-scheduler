import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Response } from '@angular/http';

import { ORIGIN_URL } from '@nguniversal/aspnetcore-engine/tokens';

import { User }  from '../models/user';

import { Observable } from 'rxjs/Observable';

@Injectable()
export class UserService {
    constructor(
        private http: HttpClient,
        @Inject(ORIGIN_URL) private baseUrl: string) { }

    getAll(): Observable<User[]> {
        return this.http.get<User[]>(`${this.baseUrl}/api/users`);
    }

    getById(id: number): Observable<User> {
        return this.http.get<User>(`${this.baseUrl}/api/users/` + id);
    }

    create(user: User) {
        return this.http.post('/api/users', user).map((response: Response) => response.json());
    }

    // update(user: User) {
    //    return this.http.put('/api/users/' + user.id, user, this.jwt()).map((response: Response) => response.json());
    // }

    // delete(id: number) {
    //    return this.http.delete('/api/users/' + id, this.jwt()).map((response: Response) => response.json());
    // }

    // private helper methods
}
