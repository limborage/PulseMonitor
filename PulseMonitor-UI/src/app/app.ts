import { Component, computed, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { SignalrService } from './services/signalr';
import { toSignal } from '@angular/core/rxjs-interop';
import { DEFAULT_METRIC, Metric } from './models/metric';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected readonly title = signal('PulseMonitor-UI');
  private signalRService = inject(SignalrService);
  private latestMetricRaw = toSignal(this.signalRService.metricReceived$);
  latestMetric = computed<Metric>(() => this.latestMetricRaw() ?? DEFAULT_METRIC);
  recentMetrics = toSignal(this.signalRService.recentMetrics, { initialValue: [] as Metric[] });
  
  ngOnInit(): void {
    console.log('App component initialized');
    console.log('Latest metric: ', this.latestMetric());
  }
}
