const weekData=[{label:'T1',posts:42,clicks:980},{label:'T2',posts:58,clicks:1240},{label:'T3',posts:71,clicks:1580},{label:'T4',posts:88,clicks:2100},{label:'T5',posts:102,clicks:2450},{label:'T6',posts:95,clicks:2280},{label:'T7',posts:120,clicks:2820}];
const maxP=Math.max(...weekData.map(d=>d.posts));
const maxC=Math.max(...weekData.map(d=>d.clicks));
document.getElementById('mainChart').innerHTML=weekData.map(d=>`<div class="bar-col"><div class="bar-stack"><div class="b-clicks" style="height:${d.clicks/maxC*80}%;min-height:4px;"></div><div class="b-posts" style="height:${d.posts/maxP*70}%;min-height:4px;"></div></div><div class="b-label">${d.label}</div></div>`).join('');
function updatePeriod(val){
  const map={
    today:{users:'1,842',posts:'12',clicks:'840',revenue:'620Kđ'},
    week:{users:'1,842',posts:'48',clicks:'5.8K',revenue:'3.4Mđ'},
    month:{users:'1,842',posts:'248',clicks:'24.8K',revenue:'14.2Mđ'},
    year:{users:'1,842',posts:'1,904',clicks:'248K',revenue:'162Mđ'}
  };
  const d=map[val];
  document.getElementById('kpi-users').textContent=d.users;
  document.getElementById('kpi-posts').textContent=d.posts;
  document.getElementById('kpi-clicks').textContent=d.clicks;
  document.getElementById('kpi-revenue').textContent=d.revenue;
}
