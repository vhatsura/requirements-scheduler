import { Component, Injectable } from '@angular/core';
import { Http, Headers, Response, RequestOptions } from '@angular/http';
import { HttpResponse } from '../models/index';
import { Observable } from 'rxjs/Observable';
import { Experiment } from '../models/index';

@Injectable()
export class ExperimentService {
    constructor(private http: Http) { }

    create(experiment: Experiment) : Observable<HttpResponse> {
        return this.http.post('/api/experiments', experiment, this.jwt()).map((response: Response) => {
            let httpResponse = new HttpResponse();
            httpResponse.status = response.status;
            httpResponse.response = response.json();
            return httpResponse;
        });
    }

    private jwt() {
        // create authorization header with jwt token
        let currentUser = JSON.parse(localStorage.getItem('currentUser'));
        if (currentUser && currentUser.token) {
            let headers = new Headers({ 'Authorization': 'Bearer ' + currentUser.token });
            return new RequestOptions({ headers: headers });
        }
    }
}