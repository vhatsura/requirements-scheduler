import { Injectable, Inject } from '@angular/core';
import { Response } from '@angular/http';

import { AuthHttp } from 'angular2-jwt';

import { ORIGIN_URL } from '../shared/constants/baseurl.constants';

import { TransferHttp } from '../../modules/transfer-http/transfer-http';
import { User }  from '../models/user';

import { Observable } from 'rxjs/Observable';

@Injectable()
export class UserService {
    constructor(
        private authHttp: AuthHttp,
        private transferHttp: TransferHttp,
        @Inject(ORIGIN_URL) private baseUrl: string) { }

    getAll(): Observable<User[]> {
        return this.transferHttp.get(`${this.baseUrl}/api/users`);
    }

    getById(id: number): Observable<User> {
        return this.transferHttp.get(`${this.baseUrl}/api/users/` + id);
    }

    create(user: User) {
        return this.authHttp.post('/api/users', user).map((response: Response) => response.json());
    }

    // update(user: User) {
    //    return this.http.put('/api/users/' + user.id, user, this.jwt()).map((response: Response) => response.json());
    // }

    // delete(id: number) {
    //    return this.http.delete('/api/users/' + id, this.jwt()).map((response: Response) => response.json());
    // }

    // private helper methods
}
