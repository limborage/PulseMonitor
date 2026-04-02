import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { scan } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Metric } from '../models/metric';

@Injectable({
  providedIn: 'root',
})
export class SignalrService {
  private hubConnection: signalR.HubConnection;
  public metricReceived$ = new BehaviorSubject<Metric | null>(null);
  public recentMetrics = this.metricReceived$.pipe(
    scan((acc, metric) => metric ? [metric, ...acc].slice(0, 10) : acc, [] as Metric[])
  );

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.signalRHubUrl)
      .withAutomaticReconnect()
      .build();

      this.startConnection();
      this.registerHandler();
  }

  private startConnection() {
    this.hubConnection.start()
    .then(() => console.log('SignalR Connected!'))
    .catch(err => console.error('Error connecting to signalR: ', err))
  }

  private registerHandler() {
    this.hubConnection.on('ReceiveMetric', (data) => {
      this.metricReceived$.next(data);
    });
  }
}
