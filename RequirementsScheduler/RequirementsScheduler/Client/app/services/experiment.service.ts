import { Component, Injectable } from '@angular/core';
import { HttpResponse } from '@angular/common/http';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { Experiment, ExperimentStatus, Test } from '../models/index';

@Injectable()
export class ExperimentService {
    constructor(private http: HttpClient) { }

    create(experiment: Experiment) : Observable<HttpResponse<Object>> {
        return this.http.post('/api/experiments', experiment, { observe: 'response' });
    }

    createTest(experiment: any) : Observable<HttpResponse<Object>> {
        return this.http.post('/api/experiments/test', experiment, { observe: 'response' });
    }

    getExperimentResults(id: string) : Observable<Test[]> {
        return this.http.get<Test[]>(`/api/experiments/${id}/result`);
    }

    getExperimentResult(id: string, testNumber: number) : Observable<Test[]> {
        return this.http.get<Test[]>(`/api/experiments/${id}/result/${testNumber}`);
    } 

    getByStatus(status: ExperimentStatus): Observable<Experiment[]> {
        return this.http.get<Experiment[]>('/api/experiments/GetByStatus/' + status);
    }
}